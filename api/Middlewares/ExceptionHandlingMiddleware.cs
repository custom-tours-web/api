using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace api.Middlewares;

/// <summary>
/// A global error handling middleware that intercepts unhandled exceptions
/// </summary>
/// <param name="next">The next request delegate in the ASP.NET Core pipeline.</param>
/// <param name="logger">The logger instance used to record exception details.</param>
[ExcludeFromCodeCoverage]
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware, wrapping the subsequent request pipeline in a try-catch block.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during the request.");

            await HandleExceptionAsync(context);
        }
    }

    /// <summary>
    /// Formats and writes a standardized JSON error response to the HTTP context.
    /// </summary>
    /// <param name="context">The current HTTP context that encountered the exception.</param>
    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            context.Response.StatusCode,
            Message = "An unexpected error occurred while processing your request. Please try again later."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
