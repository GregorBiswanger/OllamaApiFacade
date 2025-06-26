using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaApiFacade.DTOs;
using OpenAI.Chat;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace OllamaApiFacade.Extensions;

public static class SemanticKernelMapperExtensions
{
    /// <summary>
    /// Converts a <see cref="ChatRequest"/> object into a <see cref="Microsoft.SemanticKernel.ChatCompletion.ChatHistory"/> object.
    /// </summary>
    /// <param name="request">The chat request containing the messages to be converted.</param>
    /// <param name="systemMessage">An optional system message to initialize the chat history with. Defaults to an empty string.</param>
    /// <returns>A <see cref="Microsoft.SemanticKernel.ChatCompletion.ChatHistory"/> object containing the messages from the chat request.</returns>
    public static ChatHistory ToChatHistory(this ChatRequest request, string systemMessage = "")
    {
        ChatHistory chatHistory;

        if (systemMessage == "")
        {
            chatHistory = new ChatHistory();
        }
        else
        {
            chatHistory = new ChatHistory(systemMessage);
        }

        foreach (var message in request.Messages)
        {
            if (message.Role == AuthorRole.User.ToString())
            {
                chatHistory.AddUserMessage(message.Content ?? string.Empty);
            }
            else if (message.Role == AuthorRole.Assistant.ToString())
            {
                chatHistory.AddAssistantMessage(message.Content ?? string.Empty);
            }
            else if (message.Role == AuthorRole.System.ToString())
            {
                chatHistory.AddSystemMessage(message.Content ?? string.Empty);
            }
            else if (message.Role == AuthorRole.Developer.ToString())
            {
                chatHistory.AddDeveloperMessage(message.Content ?? string.Empty);
            }
        }

        return chatHistory;
    }

    /// <summary>
    /// Converts a dictionary of options into a <see cref="Microsoft.SemanticKernel.PromptExecutionSettings"/> object.
    /// </summary>
    /// <param name="options">A dictionary containing the options to be converted.</param>
    /// <returns>A <see cref="Microsoft.SemanticKernel.PromptExecutionSettings"/> object with the provided options.</returns>
    public static PromptExecutionSettings ToPromptExecutionSettings(this Dictionary<string, object> options)
    {
        return new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>(options)
        };
    }

    /// <summary>
    /// Converts a <see cref="StreamingChatMessageContent"/> object into a <see cref="OllamaApiFacade.DTOs.ChatResponse"/> object.
    /// </summary>
    /// <param name="message">The streaming chat message content to be converted.</param>
    /// <returns>A <see cref="OllamaApiFacade.DTOs.ChatResponse"/> object containing the converted message content.</returns>
    public static ChatResponse ToChatResponse(this StreamingChatMessageContent message)
    {
        return ToChatResponseInternal(message.ModelId, message.Role, message.Content, message.Metadata);
    }

    /// <summary>
    /// Converts a <see cref="Microsoft.SemanticKernel.ChatMessageContent"/> object into a <see cref="OllamaApiFacade.DTOs.ChatResponse"/> object.
    /// </summary>
    /// <param name="message">The chat message content to be converted.</param>
    /// <returns>A <see cref="OllamaApiFacade.DTOs.ChatResponse"/> object containing the converted message content.</returns>
    public static ChatResponse ToChatResponse(this ChatMessageContent? message)
    {
        return ToChatResponseInternal(message?.ModelId, message?.Role, message?.Content, message?.Metadata);
    }

    private static ChatResponse ToChatResponseInternal(string? modelId, AuthorRole? role, string? content, IReadOnlyDictionary<string, object?>? metadata)
    {
        var createdAt = GetCreatedAt(metadata);

        return new ChatResponse(
            modelId,
            createdAt,
            new Message { Role = role.ToString(), Content = content ?? "" },
            string.IsNullOrEmpty(content)
        );
    }

    private static string GetCreatedAt(IReadOnlyDictionary<string, object?>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("CreatedAt", out var createdAtValue) && createdAtValue is DateTimeOffset createdAtDateTimeOffset)
        {
            return createdAtDateTimeOffset.ToUniversalTime().ToString("o");
        }

        return DateTimeOffset.MinValue.ToUniversalTime().ToString("o");
    }

    /// <summary>
    /// Converts a <see cref="ChatMessageContent"/> object into a <see cref="OllamaApiFacade.DTOs.CompletionResponse"/> object.
    /// </summary>
    /// <param name="chatMessageContent">The chat message content to be converted.</param>
    /// <returns>A <see cref="OllamaApiFacade.DTOs.CompletionResponse"/> object containing the converted message content.</returns>
    public static CompletionResponse ToCompletionResponse(this ChatMessageContent chatMessageContent)
    {
        var chatTokenUsage = chatMessageContent.Metadata?["Usage"] as ChatTokenUsage;
        var systemFingerprint = chatMessageContent.Metadata?["SystemFingerprint"] == null;

        return new CompletionResponse(
            Id: "chatcmpl-427",
            Object: "chat.completion",
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model: chatMessageContent.ModelId,
            SystemFingerprint: systemFingerprint.ToString(),
            Choices:
            [
                new(
                    Index: 0,
                    Message: new Message
                    {
                        Role = chatMessageContent.Role.Label,
                        Content = chatMessageContent.Content
                    },
                    FinishReason: "stop"
                )
            ],
            Usage: chatTokenUsage != null ? new Usage
            {
                PromptTokens = chatTokenUsage.InputTokenCount,
                CompletionTokens = chatTokenUsage.OutputTokenCount,
                TotalTokens = chatTokenUsage.TotalTokenCount
            } : new Usage()
        );
    }
}