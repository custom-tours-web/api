using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace api.Middlewares;

/// <summary>
/// A global middleware that logs the start, completion, and execution duration of incoming HTTP requests.
/// </summary>
/// <param name="next">The next request delegate in the ASP.NET Core pipeline.</param>
/// <param name="logger">The logger instance used to record request lifecycle events.</param>
[ExcludeFromCodeCoverage]
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware, logging the incoming request path and method.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling HTTP {HttpMethod} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation("Finished HTTP {HttpMethod} {Path} with Status Code {StatusCode} in {ElapsedMilliseconds}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
