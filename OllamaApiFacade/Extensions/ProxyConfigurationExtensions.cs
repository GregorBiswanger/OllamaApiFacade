using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace OllamaApiFacade.Extensions;

public static class ProxyConfigurationExtensions
{
    /// <summary>
    /// Extension method to configure an HTTP proxy for debugging purposes.
    /// </summary>
    /// <remarks>
    /// This method sets up an HTTP client with a proxy configuration, allowing for
    /// HTTP communication to be routed through a specified proxy (e.g., for tools like Burp Suite).
    /// SSL certificate errors are ignored to simplify debugging scenarios. This setup
    /// is intended only for development and debugging purposes and should not be used in production.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the configured HTTP client will be added.</param>
    /// <param name="proxyUrl">
    /// The URL of the proxy to be used. Defaults to "http://127.0.0.1:8080".
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> with the configured HTTP client added as a singleton.
    /// </returns>
    public static IServiceCollection AddProxyForDebug(this IServiceCollection services, string proxyUrl = "http://127.0.0.1:8080")
    {
        var proxy = new WebProxy(proxyUrl)
        {
            BypassProxyOnLocal = false
        };

        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        var httpClient = new HttpClient(httpClientHandler);
        services.AddSingleton(httpClient);

        return services;
    }

    /// <summary>
    /// Adds a proxy configuration for debugging purposes to the HttpClient.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure.</param>
    /// <param name="proxyUrl">The URL of the proxy to be used. Defaults to "http://127.0.0.1:8080".</param>
    /// <returns>The configured <see cref="IHttpClientBuilder"/>.</returns>
    /// <remarks>
    /// This method sets up an HTTP client with a proxy configuration, allowing for
    /// HTTP communication to be routed through a specified proxy (e.g., for tools like Burp Suite).
    /// SSL certificate errors are ignored to simplify debugging scenarios. This setup
    /// is intended only for development and debugging purposes and should not be used in production.
    /// 
    /// Example usage:
    /// <code>
    /// builder.Services.AddHttpClient&lt;WhatsAppService&gt;(ConfigureWhatsAppHttpClient).AddProxyForDebug();
    /// </code>
    /// </remarks>
    public static IHttpClientBuilder AddProxyForDebug(this IHttpClientBuilder builder, string proxyUrl = "http://127.0.0.1:8080")
    {
        return builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            Proxy = new WebProxy(proxyUrl)
            {
                BypassProxyOnLocal = false
            },
            UseProxy = true,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    }
}
