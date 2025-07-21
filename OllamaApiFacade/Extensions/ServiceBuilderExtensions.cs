using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using OllamaApiFacade.Services;
using OpenAI;
using System.ClientModel;
using System.Diagnostics.CodeAnalysis;

namespace OllamaApiFacade.Extensions;

public static class ServiceBuilderExtensions
{
    private static string _endpoint = "http://localhost:1234/v1/";

    /// <summary>
    /// Adds support for connecting to LM Studio, which provides local LLMs (Large Language Models) and SLMs (Small Language Models), to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="model">The name of the model to use. Defaults to "lm-studio".</param>
    /// <param name="endpoint">The endpoint URL of the LM Studio. Defaults to "http://localhost:1234".</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method sets up a connection to the specified LM Studio endpoint and registers services for OpenAI chat completion with the given model.
    /// </remarks>
    public static IServiceCollection AddLmStudio(this IServiceCollection services, string model = "lm-studio", string endpoint = "http://localhost:1234/v1/")
    {
        _endpoint = endpoint;

        var uri = new Uri(endpoint);
        var openAiClientOptions = new OpenAIClientOptions { Endpoint = uri };
        var apiKeyCredential = new ApiKeyCredential("none");
        var openAiClient = new OpenAIClient(apiKeyCredential, openAiClientOptions);

        services.AddSingleton(openAiClient);
        services.AddOpenAIChatCompletion(model, openAiClient);

        return services;
    }

    /// <summary>
    /// Adds the <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> to the <see cref="IServiceCollection"/> for generating embeddings using LM-Studio.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="modelId">The name of the Embedding-Model to use for embedding generation. Defaults to "lm-studio".</param>
    /// <param name="endpoint">The endpoint URL of the LM-Studio service. Defaults to "http://localhost:1234/v1/".</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method registers the LM Studio embedding generator, enabling embedding generation via the specified model and endpoint.
    /// </remarks>
    [Experimental("SKEXP0010")]
    public static IServiceCollection AddLmStudioEmbeddingGenerator(this IServiceCollection services, string modelId = "lm-studio", string endpoint = "http://localhost:1234/v1/")
    {
        if (_endpoint != endpoint)
        {
            _endpoint = endpoint;
        }

        services.AddHttpClient("LmStudio", c => c.BaseAddress = new Uri(_endpoint));

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LmStudio");
            return new LmStudioEmbeddingGenerator(http, modelId);
        });

        return services;
    }

    /// <summary>
    /// Registers the <see cref="LmStudioEmbeddingGenerationService"/> for working with LM-Studio embeddings.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="modelId">The name of the LM-Studio model to use. Defaults to "lm-studio".</param>
    /// <param name="openAIClient">An optional <see cref="OpenAIClient"/> instance. If null, a client must be available in the service provider.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method sets up text embedding generation using the specified LM-Studio model and the provided OpenAI client.
    /// </remarks>
    [Experimental("SKEXP0010")]
    [Obsolete("Use AddLmStudioEmbeddingGenerator extension methods instead.")]
    public static IServiceCollection AddLmStudioTextEmbeddingGeneration(this IServiceCollection services,
        string modelId = "lm-studio",
        OpenAIClient? openAIClient = null)
    {
        services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            httpClient.BaseAddress = new Uri(_endpoint);

            return new LmStudioEmbeddingGenerationService(httpClient, modelId);
        });

        return services;
    }

    /// <summary>
    /// Adds support for connecting to the "AI Toolkit for Visual Studio Code" extension, providing access to locally loaded models, to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="model">The name of the model to use. Model name is case-sensitive.</param>
    /// <param name="endpoint">The endpoint URL of the AI Toolkit for Visual Studio Code. Defaults to "http://localhost:5272/v1/".</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method sets up a connection to the specified AI Toolkit for Visual Studio Code endpoint and registers services for OpenAI chat completion with the given model.
    /// The AI Toolkit for Visual Studio Code extension can be found at <see href="https://marketplace.visualstudio.com/items?itemName=ms-windows-ai-studio.windows-ai-studio"/>.
    /// </remarks>
    public static IServiceCollection AddAiToolkitVsCode(this IServiceCollection services, string model, string endpoint = "http://localhost:5272/v1/")
    {
        _endpoint = endpoint;

        var uri = new Uri(endpoint);
        var openAiClientOptions = new OpenAIClientOptions { Endpoint = uri };
        var apiKeyCredential = new ApiKeyCredential("none");
        var openAiClient = new OpenAIClient(apiKeyCredential, openAiClientOptions);

        services.AddSingleton(openAiClient);
        services.AddOpenAIChatCompletion(model, openAiClient);

        return services;
    }
}
