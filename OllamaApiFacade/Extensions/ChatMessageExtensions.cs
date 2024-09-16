using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using OllamaApiFacade.DTOs;

namespace OllamaApiFacade.Extensions;

public static class ChatMessageExtensions
{
    /// <summary>
    /// Streams chat messages to the HTTP response as JSON.
    /// </summary>
    /// <param name="messages">An asynchronous enumerable of <see cref="StreamingChatMessageContent"/> representing the chat messages.</param>
    /// <param name="response">The <see cref="HttpResponse"/> to write the messages to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method sets the response content type to "application/json" and writes each chat message to the response stream as a JSON object.
    /// </remarks>
    public static async Task StreamToResponseAsync(this IAsyncEnumerable<StreamingChatMessageContent> messages, HttpResponse response)
    {
        response.ContentType = "application/json";

        await using var writer = new StreamWriter(response.BodyWriter.AsStream(), leaveOpen: true);
        await foreach (var message in messages)
        {
            ChatResponse chatResponse = message.ToChatResponse();

            if (IsValidChatResponse(chatResponse))
            {
                var jsonResponse = JsonSerializer.Serialize(chatResponse);

                await writer.WriteAsync(jsonResponse + "\n");
                await writer.FlushAsync();
            }
        }
    }

    /// <summary>
    /// Streams chat messages to the HTTP response as JSON.
    /// </summary>
    /// <param name="messages">An asynchronous enumerable of <see cref="ChatMessageContent"/> representing the chat messages.</param>
    /// <param name="response">The <see cref="HttpResponse"/> to write the messages to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method sets the response content type to "application/json" and writes each chat message to the response stream as a JSON object.
    /// </remarks>
    public static async Task StreamToResponseAsync(this Task<IReadOnlyList<ChatMessageContent>> chatMessages, HttpResponse response)
    {
        response.ContentType = "application/json";

        await using var writer = new StreamWriter(response.BodyWriter.AsStream(), leaveOpen: true);

        var messages = await chatMessages;
        foreach (var message in messages)
        {
            ChatResponse chatResponse = message.ToChatResponse();

            if (IsValidChatResponse(chatResponse))
            {
                var jsonResponse = JsonSerializer.Serialize(chatResponse);

                await writer.WriteAsync(jsonResponse + "\n");
                await writer.FlushAsync();
            }
        }
    }

    /// <summary>
    /// Streams a single chat response to the HTTP response as JSON.
    /// </summary>
    /// <param name="chatResponse">The <see cref="ChatResponse"/> representing the chat message.</param>
    /// <param name="response">The <see cref="HttpResponse"/> to write the message to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method sets the response content type to "application/json" and writes the chat message to the response stream as a JSON object.
    /// </remarks>
    public static async Task StreamToResponseAsync(this ChatResponse chatResponse, HttpResponse response)
    {
        response.ContentType = "application/json";

        await using var writer = new StreamWriter(response.BodyWriter.AsStream(), leaveOpen: true);

        if (IsValidChatResponse(chatResponse))
        {
            var jsonResponse = JsonSerializer.Serialize(chatResponse);

            await writer.WriteAsync(jsonResponse + "\n");
            await writer.FlushAsync();
        }
    }

    /// <summary>
    /// Streams a single chat response to the HTTP response as JSON.
    /// </summary>
    /// <param name="chatMessageContent">The <see cref="ChatMessageContent"/> representing the chat message.</param>
    /// <param name="response">The <see cref="HttpResponse"/> to write the message to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method sets the response content type to "application/json" and writes the chat message to the response stream as a JSON object.
    /// </remarks>
    public static async Task StreamToResponseAsync(this ChatMessageContent chatMessageContent, HttpResponse response)
    {
        response.ContentType = "application/json";

        await using var writer = new StreamWriter(response.BodyWriter.AsStream(), leaveOpen: true);

        var chatResponse = chatMessageContent.ToChatResponse();

        if (IsValidChatResponse(chatResponse))
        {
            var jsonResponse = JsonSerializer.Serialize(chatResponse);

            await writer.WriteAsync(jsonResponse + "\n");
            await writer.FlushAsync();
        }
    }

    private static bool IsValidChatResponse(ChatResponse chatResponse)
    {
        bool hasContent = !string.IsNullOrEmpty(chatResponse.Message.Content);
        bool isNotDone = chatResponse.Done == false;
        return hasContent && isNotDone;
    }
}