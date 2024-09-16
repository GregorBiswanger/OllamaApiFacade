using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a completion response with details such as ID, object type, creation timestamp, model, system fingerprint, choices, and usage.
/// </summary>
/// <param name="Id">The unique identifier for the completion response.</param>
/// <param name="Object">The type of object returned.</param>
/// <param name="Created">The timestamp when the response was created.</param>
/// <param name="Model">The model used for the completion response.</param>
/// <param name="SystemFingerprint">The system fingerprint associated with the response.</param>
/// <param name="Choices">A list of choices included in the completion response.</param>
/// <param name="Usage">The usage details of the completion response.</param>
/// <remarks>
/// This record is used to encapsulate all necessary information for a completion response in the Ollama API facade.
/// </remarks>
public record CompletionResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("system_fingerprint")] string SystemFingerprint,
    [property: JsonPropertyName("choices")] List<Choice> Choices,
    [property: JsonPropertyName("usage")] Usage Usage
);