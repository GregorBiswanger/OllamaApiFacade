using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaApiFacade.DemoWebApi;
using OllamaApiFacade.Extensions;

const string name = "<name>";
const string keyVaultUrl = $"https://{name}.vault.azure.net";
var keyVaultHelper = new KeyVaultHelper(keyVaultUrl);
var azureOpenAiApiKey = await keyVaultHelper.GetSecretAsync("AZURE-OPENAI-API-KEY");
var azureOpenAiEndpoint = $"https://{name}.openai.azure.com";
const string deploymentGpt4 = "gpt-4";
const string deploymentGpt35 = "gpt-35-turbo-16k";

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAsLocalOllamaApi();

// With AddProxyForDebug you can use Burp Suite for communication debugging, the proxy is running on http://localhost:8080
// Support for different connectors is also important.
// In many cases, Llama is sufficient, and for fine-tuning, you might want to use GPT.
builder.Services.AddProxyForDebug()
    .AddKernel()
    .AddAzureOpenAIChatCompletion(deploymentGpt4, azureOpenAiEndpoint, azureOpenAiApiKey, modelId: deploymentGpt4)
    .AddAzureOpenAIChatCompletion(deploymentGpt35, azureOpenAiEndpoint, azureOpenAiApiKey, modelId: deploymentGpt35);

var app = builder.Build();

// Here is the central point where the magic happens in more complex scenarios.
app.MapPostApiChat(async (chatRequest, chatCompletionService, httpContext, kernel) =>
{
    var chatHistory = chatRequest.ToChatHistory();

    var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        ModelId = "gpt-4"
    };

    await chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel)
        .StreamToResponseAsync(httpContext.Response);
});

app.Run();