using Microsoft.AspNetCore.Http;

namespace OllamaApiFacade.Middlewares;

/// <summary>
/// Middleware to log JSON request bodies for analysis, particularly to understand what data is being sent from external systems like Open WebUI to our backend.
/// </summary>
public class JsonRequestLoggerMiddleware
{
    private readonly RequestDelegate _next;

    public JsonRequestLoggerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to log the JSON request body if present.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.ContentLength > 0 &&
            (context.Request.ContentType?.Contains("application/json") ?? false))
        {
            var originalBodyStream = context.Request.Body;

            try
            {
                using (var bodyStream = new MemoryStream())
                {
                    await context.Request.Body.CopyToAsync(bodyStream);
                    bodyStream.Seek(0, SeekOrigin.Begin);

                    var requestBodyText = await new StreamReader(bodyStream).ReadToEndAsync();

                    Console.WriteLine("Request JSON:");
                    Console.WriteLine(requestBodyText);

                    bodyStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = bodyStream;

                    await _next(context);
                }
            }
            finally
            {
                context.Request.Body = originalBodyStream;
            }
        }
        else
        {
            await _next(context);
        }
    }
}