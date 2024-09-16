using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace OllamaApiFacade.Extensions;

public static class WebHostBuilderExtensions
{
    /// <summary>
    /// Configures the application to run as a local Ollama API by automatically setting the URL to "http://localhost:11434".
    /// This is intended for development scenarios where a local Ollama backend is needed for clients.
    /// 
    /// If you wish to provide your own custom URL or configuration, this method is not required. You can modify the URL or other settings 
    /// in the "launchSettings.json" file of your project. By doing so, this method will be overridden and unnecessary.
    /// 
    /// The primary purpose of this method is to ensure that any client expecting an Ollama backend will be able to access the predefined 
    /// local API address at "http://localhost:11434".
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder used to configure the web host.</param>
    /// <returns>Returns the modified WebApplicationBuilder instance.</returns>
    public static WebApplicationBuilder ConfigureAsLocalOllamaApi(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseUrls("http://localhost:11434");

        return builder;
    }
}