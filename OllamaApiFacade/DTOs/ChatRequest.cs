using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a chat request with details such as chat ID, messages, model, options, session ID, and streaming flag.
/// </summary>
/// <param name="ChatId">The unique identifier for the chat.</param>
/// <param name="Id">The unique identifier for the request.</param>
/// <param name="Messages">A list of messages included in the chat request.</param>
/// <param name="Model">The model to be used for the chat.</param>
/// <param name="Options">A dictionary of options for the chat request.</param>
/// <param name="SessionId">The unique identifier for the session.</param>
/// <param name="Stream">A flag indicating whether the response should be streamed.</param>
/// <remarks>
/// This record is used to encapsulate all necessary information for a chat request in the Ollama API facade.
/// </remarks>
public record ChatRequest(
    [property: JsonPropertyName("chat_id")] string ChatId,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("messages")] List<Message> Messages,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("options")] Dictionary<string, object> Options,
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("stream")] bool Stream
);