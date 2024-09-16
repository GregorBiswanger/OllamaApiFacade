using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a message in a chat, including the role and content.
/// </summary>
/// <remarks>
/// This record is used to encapsulate individual messages in chat requests and responses within the Ollama API facade.
/// </remarks>
public record Message
{
    /// <summary>
    /// Gets the role of the message sender (e.g., user, assistant).
    /// </summary>
    [property: JsonPropertyName("role")]
    public string? Role { get; init; } = "";

    /// <summary>
    /// Gets the content of the message.
    /// </summary>
    [property: JsonPropertyName("content")]
    public string? Content { get; init; } = "";
}