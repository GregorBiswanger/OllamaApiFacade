using Microsoft.AspNetCore.Http;
using OllamaApiFacade.DTOs;

namespace OllamaApiFacade.Extensions;

public static class HttpContextExtensions
{
    private const string ChatRequestKey = "ChatRequest";

    /// <summary>
    /// Stores the ChatRequest in HttpContext.Items.
    /// </summary>
    /// <param name="context">The current HttpContext.</param>
    /// <param name="chatRequest">The ChatRequest to be stored.</param>
    /// <exception cref="ArgumentNullException">Thrown if the context or chatRequest is null.</exception>
    public static void SetChatRequest(this HttpContext context, ChatRequest chatRequest)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (chatRequest == null) throw new ArgumentNullException(nameof(chatRequest));

        context.Items[ChatRequestKey] = chatRequest;
    }

    /// <summary>
    /// Retrieves the ChatRequest from HttpContext.Items.
    /// </summary>
    /// <param name="context">The current HttpContext.</param>
    /// <returns>The stored ChatRequest, or null if not present.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the context is null.</exception>
    public static ChatRequest? GetChatRequest(this HttpContext? context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (context.Items.TryGetValue(ChatRequestKey, out var value) && value is ChatRequest chatRequest)
        {
            return chatRequest;
        }
        return null;
    }
}