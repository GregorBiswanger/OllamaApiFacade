using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a completion request with details such as model, messages, streaming flag, and maximum tokens.
/// </summary>
/// <param name="Model">The model to be used for the completion request.</param>
/// <param name="Messages">A list of messages included in the completion request.</param>
/// <param name="Stream">A flag indicating whether the response should be streamed.</param>
/// <param name="MaxTokens">The maximum number of tokens for the completion.</param>
/// <remarks>
/// This record is used to encapsulate all necessary information for a completion request in the Ollama API facade.
/// </remarks>
public record CompletionRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<Message> Messages,
    [property: JsonPropertyName("stream")] bool Stream,
    [property: JsonPropertyName("max_tokens")] int MaxTokens
);