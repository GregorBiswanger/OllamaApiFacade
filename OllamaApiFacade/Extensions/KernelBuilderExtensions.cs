using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using OllamaApiFacade.Services;
using OpenAI;

namespace OllamaApiFacade.Extensions;

public static class KernelBuilderExtensions
{
    private static string _endpoint = "http://localhost:1234/v1/";

    /// <summary>
    /// Adds support for connecting to LM Studio, which provides local LLMs (Large Language Models) and SLMs (Small Language Models), to the <see cref="IKernelBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> to configure.</param>
    /// <param name="model">The name of the model to use. Defaults to "lm-studio".</param>
    /// <param name="endpoint">The endpoint URL of the LM Studio. Defaults to "http://localhost:1234".</param>
    /// <returns>The configured <see cref="IKernelBuilder"/>.</returns>
    /// <remarks>
    /// This method sets up a connection to the specified LM Studio endpoint and configures the kernel builder to use OpenAI chat completion with the provided model.
    /// </remarks>
    public static IKernelBuilder AddLmStudio(this IKernelBuilder builder, string model = "lm-studio", string endpoint = "http://localhost:1234/v1/")
    {
        _endpoint = endpoint;

        var uri = new Uri(endpoint);
        var openAiClientOptions = new OpenAIClientOptions { Endpoint = uri };
        var apiKeyCredential = new ApiKeyCredential("none");
        var openAiClient = new OpenAIClient(apiKeyCredential, openAiClientOptions);

        builder.Services.AddSingleton(openAiClient);
        builder.AddOpenAIChatCompletion(model, openAiClient);

        return builder;
    }

    /// <summary>
    /// Adds the <see cref="OpenAITextEmbeddingGenerationService"/> to the <see cref="IKernelBuilder.Services"/> for working with LM-Studio embeddings.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="modelId">The name of the LM-Studio model to use. Defaults to "lm-studio".</param>
    /// <param name="openAIClient">The <see cref="OpenAIClient"/> to use for the service. If null, one must be available in the service provider when this service is resolved.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    /// <remarks>
    /// This method configures the kernel builder to use the specified LM-Studio model for text embedding generation, utilizing the provided OpenAI client and optional service ID and dimensions.
    /// </remarks>
    [Experimental("SKEXP0010")]
    public static IKernelBuilder AddLmStudioTextEmbeddingGeneration(this IKernelBuilder builder,
        string modelId = "lm-studio",
        OpenAIClient? openAIClient = null)
    {
        builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            httpClient.BaseAddress = new Uri(_endpoint);

            return new LmStudioEmbeddingGenerationService(httpClient, modelId);
        });

        return builder;
    }
}