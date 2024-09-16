using Microsoft.SemanticKernel;
using OpenAI;

namespace OllamaApiFacade.Extensions;

public static class KernelBuilderExtensions
{
    /// <summary>
    /// Adds support for connecting to LM Studio, which provides local LLMs (Large Language Models) and SLMs (Small Language Models), to the <see cref="IKernelBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> to configure.</param>
    /// <param name="model">The name of the model to use. Defaults to "none".</param>
    /// <param name="endpoint">The endpoint URL of the LM Studio. Defaults to "http://localhost:1234".</param>
    /// <returns>The configured <see cref="IKernelBuilder"/>.</returns>
    /// <remarks>
    /// This method sets up a connection to the specified LM Studio endpoint and configures the kernel builder to use OpenAI chat completion with the provided model.
    /// </remarks>
    public static IKernelBuilder AddLmStudio(this IKernelBuilder builder, string model = "none", string endpoint = "http://localhost:1234")
    {
        var uri = new Uri(endpoint);
        var openAiClientOptions = new OpenAIClientOptions { Endpoint = uri };
        var openAiClient = new OpenAIClient("none", openAiClientOptions);

        builder.AddOpenAIChatCompletion(model, openAiClient);

        return builder;
    }
}