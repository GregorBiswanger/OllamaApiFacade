using System.Net.Http.Json;
using Microsoft.Extensions.AI;

namespace OllamaApiFacade.Services;

/// <summary>
/// <para>
/// An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> implementation that talks to a
/// local <strong>LM Studio</strong> instance. The generator explicitly requests embeddings in
/// <c>float</c> format (<c>encoding_format="float"</c>) and converts the response into the
/// abstractions defined in <c>Microsoft.Extensions.AI</c> so that the rest of the
/// <see cref="https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai">.NET AI
/// stack</see> can consume it transparently.
/// </para>
/// <para>
/// <b>Why a custom generator?</b>  The official LM Studio REST endpoint currently ignores the
/// <c>encoding_format</c> parameter sent by the Microsoft SDK and always returns raw
/// <c>float[]</c>. The default <c>OpenAIEmbeddingGenerator</c> in
/// <c>Microsoft.Extensions.AI.OpenAI</c> therefore fails with
/// <see cref="FormatException"/>. This class bypasses that limitation by requesting floats on
/// purpose and mapping the response by hand.
/// </para>
/// <para>
/// <b>Thread‑safety:</b>  Instances are designed to be <em>stateless</em> and therefore safe for
/// concurrent use, matching the recommendations in the remarks section of
/// <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/>.
/// </para>
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description>The generator supports multiple inputs in a single request.</description></item>
///   <item><description>The returned <see cref="GeneratedEmbeddings{TEmbedding}"/> contains token‑usage information as provided by LM Studio.</description></item>
///   <item><description>This implementation is intended <strong>only</strong> for local LM Studio deployments. No authentication layer is built in.</description></item>
/// </list>
/// </remarks>
public sealed class LmStudioEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly HttpClient _http;
    private readonly string _modelId;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LmStudioEmbeddingGenerator"/> class.
    /// </summary>
    /// <param name="http">
    /// The <see cref="HttpClient"/> the generator should use. It will be disposed together
    /// with the generator unless the client is managed elsewhere (for example, via
    /// <c>IHttpClientFactory</c>).
    /// </param>
    /// <param name="modelId">The model identifier understood by your LM Studio install.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="http"/> or <paramref name="modelId"/> is <see langword="null"/>.
    /// </exception>
    public LmStudioEmbeddingGenerator(HttpClient http, string modelId)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        _http.BaseAddress ??= new Uri("http://localhost:1234/v1/");
    }

    /// <inheritdoc />
    /// <summary>
    /// Generates a set of embeddings for the supplied <paramref name="values"/> by delegating
    /// the work to LM Studio.
    /// </summary>
    /// <param name="values">The texts to embed.</param>
    /// <param name="options">
    /// Embedding‑generation options supplied by the caller. Currently ignored because LM Studio
    /// does not honor them.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="GeneratedEmbeddings{TEmbedding}"/> instance containing the vectors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        var request = new
        {
            input = values.ToArray(),
            model = _modelId,
            encoding_format = "float"
        };

        using var response = await _http.PostAsJsonAsync("embeddings", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var lmStudioResponse = await response.Content.ReadFromJsonAsync<LmResponse>(cancellationToken);

        var vectors = lmStudioResponse!.data.Select(item =>
            new Embedding<float>(item.embedding.ToArray())
            {
                ModelId = lmStudioResponse.model
            }).ToList();

        var result = new GeneratedEmbeddings<Embedding<float>>(vectors)
        {
            Usage = new UsageDetails
            {
                InputTokenCount = lmStudioResponse.usage.prompt_tokens,
                OutputTokenCount = lmStudioResponse.usage.output_tokens,
                TotalTokenCount = lmStudioResponse.usage.total_tokens
            }
        };

        return result;
    }

    /// <inheritdoc />
    /// <summary>
    /// Returns the underlying <see cref="HttpClient"/> when requested via
    /// <see cref="IEmbeddingGenerator.GetService(Type, object?)"/>. This allows advanced callers
    /// to customize headers or observe requests.
    /// </summary>
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType == typeof(HttpClient) ? _http : null;

    /// <inheritdoc />
    /// <remarks>The method is idempotent.</remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _http.Dispose();
    }

    private sealed record LmResponse(List<Item> data, string model, Usage usage);
    private sealed record Item(List<float> embedding);
    private sealed record Usage(int prompt_tokens, int output_tokens, int total_tokens);
}
