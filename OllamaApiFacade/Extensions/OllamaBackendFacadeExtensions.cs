using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaApiFacade.DTOs;

namespace OllamaApiFacade.Extensions;

public static class OllamaBackendFacadeExtensions
{
    /// <summary>
    /// Configures the <see cref="WebApplication"/> to use the Ollama Backend Facade, intercepting and handling the original routing paths from the Ollama API server.
    /// This setup supports integration with tools such as Open Web UI.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <param name="modelName">The name of the model to use. Defaults to "dotnetapi".</param>
    /// <param name="ollamaApiVersion">The version of the Ollama API to use. Defaults to "0.1.33".</param>
    /// <returns>The configured <see cref="IApplicationBuilder"/>.</returns>
    public static IEndpointRouteBuilder MapOllamaBackendFacade(this IEndpointRouteBuilder endpoints, string modelName = "dotnetapi", string ollamaApiVersion = "0.1.33")
    {
        if (IsRouteNotRegistered(endpoints, "/api/tags", "GET"))
        {
            endpoints.MapGet("/api/tags", () =>
            {
                var response = new
                {
                    models = new[]
                    {
                        new
                        {
                            name = modelName,
                            model = $"{modelName}:latest",
                            digest = "a6990ed6be412c6a217614b0ec8e9cd6800a743d5dd7e1d7fbe9df09e61d5615",
                        }
                    }
                };
                return Results.Json(response);
            });
        }

        if (IsRouteNotRegistered(endpoints, "/api/version", "GET"))
        {
            endpoints.MapGet("/api/version", () =>
            {
                var response = new
                {
                    version = ollamaApiVersion
                };
                return Results.Json(response);
            });
        }

        if (IsRouteNotRegistered(endpoints, "/v1/chat/completions", "POST"))
        {
            endpoints.MapPost("/v1/chat/completions",
                async (CompletionRequest completionRequest, IChatCompletionService chatCompletion,
                    HttpContext context) =>
                {
                    var promptExecutionSettings = new PromptExecutionSettings
                    {
                        ExtensionData = new Dictionary<string, object>
                        {
                            { "max_tokens", completionRequest.MaxTokens }
                        }
                    };

                    var message = completionRequest.Messages.First();
                    if (message.Content == null)
                    {
                        return Results.BadRequest();
                    }

                    var messages =
                        await chatCompletion.GetChatMessageContentsAsync(message.Content, promptExecutionSettings);
                    var response = messages.First().ToCompletionResponse();

                    return Results.Json(response);
                });
        }

        return endpoints;
    }

    private static bool IsRouteNotRegistered(IEndpointRouteBuilder endpoints, string pattern, string method)
    {
        var routeEndpoint = endpoints.DataSources.SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .FirstOrDefault(e =>
                string.Equals(e.RoutePattern.RawText, pattern, StringComparison.OrdinalIgnoreCase) &&
                e.Metadata.OfType<HttpMethodMetadata>().Any(m => m.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase)));

        return routeEndpoint == null;
    }
}