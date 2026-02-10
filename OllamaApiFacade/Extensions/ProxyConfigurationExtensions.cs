using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Net;

namespace OllamaApiFacade.Extensions;

public static class ProxyConfigurationExtensions
{
    public sealed class BurpProxyOptions
    {
        public string ProxyUrl { get; set; } = "http://127.0.0.1:8080";
        public string[] ExcludedHosts { get; set; } = Array.Empty<string>();
        public int[] ExcludedPorts { get; set; } = new[] { 6334 };

        /// <summary>
        /// Automatically exclude gRPC traffic by detecting HTTP/2 protocol usage.
        /// Default: true
        /// </summary>
        public bool AutoExcludeGrpc { get; set; } = true;

        /// <summary>
        /// Ignore SSL certificate errors (required for Burp Suite interception).
        /// Default: true
        /// </summary>
        public bool IgnoreSslErrors { get; set; } = true;

        /// <summary>
        /// Set environment proxy variables (HTTP_PROXY, HTTPS_PROXY) for libraries that use them.
        /// Default: true
        /// </summary>
        public bool SetEnvironmentProxyVariables { get; set; } = true;
    }

    /// <summary>
    /// Adds proxy configuration for debugging purposes with default settings.
    /// Default: Proxy URL is http://127.0.0.1:8080, Port 6334 is excluded.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddProxyForDebug();
    /// </code>
    /// </example>
    public static IServiceCollection AddProxyForDebug(this IServiceCollection services)
    {
        return services.AddProxyForDebug(_ => { });
    }

    /// <summary>
    /// Adds proxy configuration for debugging purposes with custom proxy URL and excluded ports.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="proxyUrl">The proxy URL. Default: http://127.0.0.1:8080</param>
    /// <param name="excludedPorts">Ports to exclude from proxy (e.g., gRPC ports). Default: 6334</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddProxyForDebug("http://127.0.0.1:8080", excludedPorts: new[] { 6334, 5001 });
    /// </code>
    /// </example>
    public static IServiceCollection AddProxyForDebug(
        this IServiceCollection services,
        string proxyUrl = "http://127.0.0.1:8080",
        int[]? excludedPorts = null)
    {
        return services.AddProxyForDebug(options =>
        {
            options.ProxyUrl = proxyUrl;
            
            if (excludedPorts != null && excludedPorts.Length > 0)
            {
                options.ExcludedPorts = excludedPorts;
            }
        });
    }

    /// <summary>
    /// Adds proxy configuration for debugging purposes with full customization.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure proxy options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddProxyForDebug(options => 
    /// {
    ///     options.ProxyUrl = "http://127.0.0.1:8080";
    ///     options.ExcludedPorts = new[] { 6334, 5001, 5051 };
    ///     options.ExcludedHosts = new[] { "grpc-backend.internal" };
    ///     options.AutoExcludeGrpc = true;
    ///     options.IgnoreSslErrors = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddProxyForDebug(
        this IServiceCollection services,
        Action<BurpProxyOptions> configure)
    {
        var options = new BurpProxyOptions();
        configure(options);

        var proxy = new PortAwareWebProxy(
            options.ProxyUrl,
            options.ExcludedHosts,
            options.ExcludedPorts,
            options.AutoExcludeGrpc);

        var ignoreSslErrors = options.IgnoreSslErrors;
        var proxyInstance = proxy;

        // 1) Global defaults (for libraries that create HttpClients internally)
        HttpClient.DefaultProxy = proxyInstance;
        WebRequest.DefaultWebProxy = proxyInstance;

        // 2) Environment variables (affects many stacks/libraries)
        if (options.SetEnvironmentProxyVariables)
        {
            Environment.SetEnvironmentVariable("HTTP_PROXY", options.ProxyUrl);
            Environment.SetEnvironmentVariable("HTTPS_PROXY", options.ProxyUrl);
            Environment.SetEnvironmentVariable("http_proxy", options.ProxyUrl);
            Environment.SetEnvironmentVariable("https_proxy", options.ProxyUrl);
        }

        // 3) Register Singleton HttpClient for direct injection
        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxyInstance,
            UseProxy = true
        };

        if (ignoreSslErrors)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var httpClient = new HttpClient(httpClientHandler);
        services.AddSingleton(httpClient);

        // 4) HttpClientFactory (for Named/Typed clients)
        services.ConfigureAll<HttpClientFactoryOptions>(factoryOptions =>
        {
            factoryOptions.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                // Handle HttpClientHandler (legacy)
                if (builder.PrimaryHandler is HttpClientHandler httpHandler)
                {
                    httpHandler.UseProxy = true;
                    httpHandler.Proxy = proxyInstance;

                    // Ignore SSL errors for Burp Suite
                    if (ignoreSslErrors)
                    {
                        httpHandler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                }
                // Handle SocketsHttpHandler (modern, .NET Core 2.1+)
                else if (builder.PrimaryHandler is SocketsHttpHandler socketsHandler)
                {
                    socketsHandler.UseProxy = true;
                    socketsHandler.Proxy = proxyInstance;

                    // Ignore SSL errors for Burp Suite
                    if (ignoreSslErrors)
                    {
                        socketsHandler.SslOptions.RemoteCertificateValidationCallback =
                            (sender, certificate, chain, sslPolicyErrors) => true;
                    }
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Custom WebProxy implementation with port and host-based bypass logic and gRPC auto-detection.
    /// </summary>
    private sealed class PortAwareWebProxy : IWebProxy
    {
        private readonly Uri _proxyUri;
        private readonly string[] _excludedHosts;
        private readonly int[] _excludedPorts;
        private readonly bool _autoExcludeGrpc;

        // Common gRPC ports for auto-detection
        private static readonly int[] CommonGrpcPorts = [5001, 5051, 50051, 9090];

        public PortAwareWebProxy(
            string proxyUrl,
            string[] excludedHosts,
            int[] excludedPorts,
            bool autoExcludeGrpc)
        {
            _proxyUri = new Uri(proxyUrl);
            _excludedHosts = excludedHosts
                .Select(h => h.Trim())
                .Where(h => h.Length > 0)
                .ToArray();
            _excludedPorts = excludedPorts ?? [];
            _autoExcludeGrpc = autoExcludeGrpc;
        }

        public ICredentials? Credentials { get; set; }

        public Uri GetProxy(Uri destination) => _proxyUri;

        public bool IsBypassed(Uri destination)
        {
            // Check excluded hosts
            if (_excludedHosts.Any(h =>
                destination.Host.Equals(h, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check explicitly excluded ports
            if (_excludedPorts.Contains(destination.Port))
            {
                return true;
            }

            // Auto-detect gRPC traffic based on common patterns
            if (_autoExcludeGrpc)
            {
                // Check if it's using a common gRPC port
                if (CommonGrpcPorts.Contains(destination.Port))
                {
                    return true;
                }

                // Check if the host suggests gRPC usage
                if (destination.Host.Contains("grpc", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Check if path suggests gRPC (gRPC uses specific path patterns)
                if (destination.AbsolutePath.StartsWith("/grpc.", StringComparison.OrdinalIgnoreCase) ||
                    destination.AbsolutePath.Contains(".proto", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Extension method for IHttpClientBuilder to add proxy configuration.
    /// Useful for configuring specific named or typed clients.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="proxyUrl">The proxy URL. Default: http://127.0.0.1:8080</param>
    /// <param name="excludedPorts">Ports to exclude from proxy. Default: 6334</param>
    /// <param name="ignoreSslErrors">Whether to ignore SSL certificate errors. Default: true</param>
    /// <returns>The HTTP client builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddHttpClient&lt;WeatherService&gt;()
    ///     .AddProxyForDebug();
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddProxyForDebug(
        this IHttpClientBuilder builder,
        string proxyUrl = "http://127.0.0.1:8080",
        int[]? excludedPorts = null,
        bool ignoreSslErrors = true)
    {
        var actualExcludedPorts = excludedPorts ?? new[] { 6334 };

        Environment.SetEnvironmentVariable("HTTP_PROXY", proxyUrl);
        Environment.SetEnvironmentVariable("HTTPS_PROXY", proxyUrl);

        return builder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler
            {
                Proxy = new PortAwareWebProxy(
                    proxyUrl,
                    Array.Empty<string>(),
                    actualExcludedPorts,
                    autoExcludeGrpc: true),
                UseProxy = true
            };

            if (ignoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        });
    }
}