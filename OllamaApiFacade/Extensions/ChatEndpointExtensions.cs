using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaApiFacade.DTOs;

namespace OllamaApiFacade.Extensions;

public static class ChatEndpointExtensions
{
    /// <summary>
    /// Maps a POST endpoint "/api/chat" for handling chat requests, intercepting normal chat dialogs directed to Ollama.
    /// This setup supports integration with tools such as Open Web UI for handling conversations.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure.</param>
    /// <param name="handler">A function that processes the chat request, chat completion service, 
    /// kernel, and HTTP context as parameters.</param>
    /// <returns>The configured <see cref="IEndpointConventionBuilder"/>.</returns>
    public static IEndpointConventionBuilder MapPostApiChat(this IEndpointRouteBuilder endpoints, Func<ChatRequest, IChatCompletionService, HttpContext, Kernel, Task> handler)
    {
        endpoints.MapOllamaBackendFacade();

        return endpoints.MapPost("/api/chat", async context =>
        {
            var chatRequest = await context.Request.ReadFromJsonAsync<ChatRequest>();
            if (chatRequest == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request body");
                return;
            }

            var chatCompletion = context.RequestServices.GetRequiredService<IChatCompletionService>();
            var kernel = context.RequestServices.GetRequiredService<Kernel>();

            await handler(chatRequest, chatCompletion, context, kernel);
        });
    }

    /// <summary>
    /// Maps a POST endpoint for handling chat requests, intercepting normal chat dialogs directed to Ollama.
    /// This setup supports integration with tools such as Open Web UI for handling conversations.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure.</param>
    /// <param name="handler">A function that processes the chat request, chat completion service, and kernel, 
    /// but does not require the HTTP context.</param>
    /// <returns>The configured <see cref="IEndpointConventionBuilder"/>.</returns>
    public static IEndpointConventionBuilder MapPostApiChat(this IEndpointRouteBuilder endpoints, Func<ChatRequest, IChatCompletionService, Kernel, Task> handler)
    {
        return endpoints.MapPostApiChat(async (chatRequest, chatCompletion, context, kernel) =>
        {
            await handler(chatRequest, chatCompletion, kernel);
        });
    }

    /// <summary>
    /// Maps a POST endpoint for handling chat requests, intercepting normal chat dialogs directed to Ollama.
    /// This setup supports integration with tools such as Open Web UI for handling conversations.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure.</param>
    /// <param name="handler">A function that processes the chat request, chat completion service, and HttpContext, 
    /// but does not require the HTTP context.</param>
    /// <returns>The configured <see cref="IEndpointConventionBuilder"/>.</returns>
    public static IEndpointConventionBuilder MapPostApiChat(this IEndpointRouteBuilder endpoints, Func<ChatRequest, IChatCompletionService, HttpContext, Task> handler)
    {
        return endpoints.MapPostApiChat(async (chatRequest, chatCompletion, context, kernel) =>
        {
            await handler(chatRequest, chatCompletion, context);
        });
    }

    /// <summary>
    /// Maps a POST endpoint for handling chat requests, intercepting normal chat dialogs directed to Ollama.
    /// This setup supports integration with tools such as Open Web UI for handling conversations.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to configure.</param>
    /// <param name="handler">A function that processes only the chat request and chat completion service, 
    /// without requiring access to the kernel or HTTP context.</param>
    /// <returns>The configured <see cref="IEndpointConventionBuilder"/>.</returns>
    public static IEndpointConventionBuilder MapPostApiChat(this IEndpointRouteBuilder endpoints, Func<ChatRequest, IChatCompletionService, Task> handler)
    {
        return endpoints.MapPostApiChat(async (chatRequest, chatCompletion, context, kernel) =>
        {
            await handler(chatRequest, chatCompletion);
        });
    }
}

