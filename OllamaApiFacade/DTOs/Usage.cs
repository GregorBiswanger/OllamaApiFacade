using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents the usage details of a completion response, including prompt tokens, completion tokens, and total tokens.
/// </summary>
/// <remarks>
/// This record is used to encapsulate token usage information in the Ollama API facade.
/// </remarks>
public record Usage
{
    /// <summary>
    /// Gets the number of tokens used in the prompt.
    /// </summary>
    [property: JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    /// <summary>
    /// Gets the number of tokens used in the completion.
    /// </summary>
    [property: JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    /// <summary>
    /// Gets the total number of tokens used.
    /// </summary>
    [property: JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}

