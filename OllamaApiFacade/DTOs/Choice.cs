using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a choice in a chat response, including the index, message, and reason for finishing.
/// </summary>
/// <param name="Index">The index of the choice.</param>
/// <param name="Message">The message associated with the choice.</param>
/// <param name="FinishReason">The reason why the choice was finished.</param>
/// <remarks>
/// This record is used to encapsulate individual choices in a chat response within the Ollama API facade.
/// </remarks>
public record Choice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] Message Message,
    [property: JsonPropertyName("finish_reason")] string FinishReason
);