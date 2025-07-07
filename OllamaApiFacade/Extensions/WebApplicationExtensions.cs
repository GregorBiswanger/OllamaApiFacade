using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace OllamaApiFacade.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds HTTPS redirection for all requests except POST /api/chat, /api/tags, /api/version, and /v1/chat/completions.
    /// </summary>
    public static IApplicationBuilder UseHttpsRedirectionExceptOllamaApi(this IApplicationBuilder app)
    {
        return app.UseWhen(
            ctx =>
            {
                var path = ctx.Request.Path;
                var method = ctx.Request.Method;
                if (method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
                {
                    if (path.Equals("/api/chat", StringComparison.OrdinalIgnoreCase) ||
                        path.Equals("/v1/chat/completions", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                if (path.Equals("/api/tags", StringComparison.OrdinalIgnoreCase) ||
                    path.Equals("/api/version", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            },
            branch => branch.UseHttpsRedirection());
    }
}