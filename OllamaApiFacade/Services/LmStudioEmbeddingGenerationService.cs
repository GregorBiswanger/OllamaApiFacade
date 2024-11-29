using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace OllamaApiFacade.Services;

/// <summary>
/// Service for generating text embeddings using LM Studio.
/// </summary>
[Experimental("SKEXP0001")]
public class LmStudioEmbeddingGenerationService : ITextEmbeddingGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LmStudioEmbeddingGenerationService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="modelId">The model identifier to use for embedding generation.</param>
    public LmStudioEmbeddingGenerationService(HttpClient httpClient, string modelId)
    {
        _httpClient = httpClient;
        _modelId = modelId;
    }

    /// <summary>
    /// Generates an embedding for a single input text.
    /// </summary>
    /// <param name="input">The input text to generate an embedding for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of floats representing the embedding.</returns>
    public async Task<IList<float>> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var requestContent = new
        {
            model = _modelId,
            input = input
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/embeddings", requestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<LmStudioEmbeddingResponse>(cancellationToken: cancellationToken);
        var embeddings = responseData?.Data?.FirstOrDefault()?.Embedding;

        if (embeddings == null)
        {
            throw new InvalidOperationException("No embeddings found in the response.");
        }

        return embeddings;
    }

    /// <summary>
    /// Generates embeddings for a list of input texts.
    /// </summary>
    /// <param name="data">The list of input texts.</param>
    /// <param name="kernel">Optional kernel parameter.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of embeddings, each represented as ReadOnlyMemory of floats.</returns>
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();

        foreach (var text in data)
        {
            var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
            embeddings.Add(embedding.ToArray());
        }

        return embeddings;
    }

    /// <summary>
    /// Gets the attributes associated with the embedding generation service.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Attributes { get; }
}

/// <summary>
/// Represents the response from the LM Studio embedding API.
/// </summary>
public class LmStudioEmbeddingResponse
{
    /// <summary>
    /// Gets or sets the list of embedding data.
    /// </summary>
    public List<EmbeddingData> Data { get; set; }
}

/// <summary>
/// Represents the embedding data for a single input.
/// </summary>
public class EmbeddingData
{
    /// <summary>
    /// Gets or sets the embedding vector.
    /// </summary>
    public List<float> Embedding { get; set; }
}