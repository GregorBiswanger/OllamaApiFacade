using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OllamaApiFacade.DemoWebApi.Plugins;
using OllamaApiFacade.DTOs;
using OllamaApiFacade.Extensions;
using System.Text.Json;
using OllamaApiFacade.DemoWebApi;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureAsLocalOllamaApi();

//const string name = "<NAME>";
//const string keyVaultUrl = $"https://{name}.vault.azure.net";
//var keyVaultHelper = new KeyVaultHelper(keyVaultUrl);
//var azureOpenAiApiKey = await keyVaultHelper.GetSecretAsync("AZURE-OPENAI-API-KEY");
////var openAiApiKey = await keyVaultHelper.GetSecretAsync("OPENAI-API-KEY");
//var azureOpenAiEndpoint = $"https://{name}.openai.azure.com";
//const string azureOpenAiDeploymentName = "gpt-4";

//builder.Services.AddKernel()
//    .AddAzureOpenAIChatCompletion(azureOpenAiDeploymentName, azureOpenAiEndpoint, azureOpenAiApiKey)
//    .Plugins.AddFromType<TimeInformationPlugin>();

//builder.Services.AddKernel().AddOpenAIChatCompletion("gpt-4o", openAiApiKey)
//    .Plugins.AddFromType<TimeInformationPlugin>();

builder.Services.AddKernel().AddAiToolkitVsCode(model: "Phi-3-mini-128k-cpu-int4-rtn-block-32-acc-level-4-onnx")
    .Plugins.AddFromType<TimeInformationPlugin>();

var app = builder.Build();

app.MapPostApiChat(async (chatRequest, chatCompletionService, httpContext, kernel) =>
{
    var chatHistory = chatRequest.ToChatHistory();

    var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    await chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel)
        .StreamToResponseAsync(httpContext.Response);
});

app.Run();