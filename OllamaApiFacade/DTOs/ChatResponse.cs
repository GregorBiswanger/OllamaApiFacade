using System.Text.Json.Serialization;

namespace OllamaApiFacade.DTOs;

/// <summary>
/// Represents a chat response with details such as model, creation time, message, completion status, and various durations and counts.
/// </summary>
/// <param name="Model">The model used for the chat response.</param>
/// <param name="CreatedAt">The timestamp when the response was created.</param>
/// <param name="Message">The message included in the chat response.</param>
/// <param name="Done">A flag indicating whether the chat response is complete.</param>
/// <param name="TotalDuration">The total duration of the chat response processing.</param>
/// <param name="LoadDuration">The duration taken to load the model.</param>
/// <param name="PromptEvalCount">The number of prompt evaluations performed.</param>
/// <param name="PromptEvalDuration">The duration of prompt evaluations.</param>
/// <param name="EvalCount">The number of evaluations performed.</param>
/// <param name="EvalDuration">The duration of evaluations.</param>
/// <remarks>
/// This record is used to encapsulate all necessary information for a chat response in the Ollama API facade.
/// </remarks>
public record ChatResponse(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("message")] Message Message,
    [property: JsonPropertyName("done")] bool Done,
    [property: JsonPropertyName("total_duration")] long? TotalDuration = null,
    [property: JsonPropertyName("load_duration")] long? LoadDuration = null,
    [property: JsonPropertyName("prompt_eval_count")] int? PromptEvalCount = null,
    [property: JsonPropertyName("prompt_eval_duration")] long? PromptEvalDuration = null,
    [property: JsonPropertyName("eval_count")] int? EvalCount = null,
    [property: JsonPropertyName("eval_duration")] long? EvalDuration = null
);