using AgileFlow.Application.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception after response started for {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}. Returned {StatusCode}.",
                context.Request.Method,
                context.Request.Path,
                MapStatusCode(exception));

            context.Response.Clear();
            context.Response.StatusCode = MapStatusCode(exception);

            // EmailNotVerifiedException gets an augmented body so the frontend can detect it
            if (exception is EmailNotVerifiedException emailEx)
            {
                await context.Response.WriteAsJsonAsync(new
                {
                    message = emailEx.Message,
                    requiresEmailConfirmation = true,
                    email = emailEx.Email,
                });
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new { message = MapMessage(exception) });
            }
        }
    }

    private static int MapStatusCode(Exception exception) => exception switch
    {
        KeyNotFoundException        => StatusCodes.Status404NotFound,
        EmailNotVerifiedException   => StatusCodes.Status403Forbidden,
        UnauthorizedAccessException => StatusCodes.Status403Forbidden,
        ArgumentException           => StatusCodes.Status400BadRequest,
        InvalidOperationException   => StatusCodes.Status409Conflict,
        SecurityTokenException      => StatusCodes.Status401Unauthorized,
        _                           => StatusCodes.Status500InternalServerError,
    };

    private static string MapMessage(Exception exception) => exception switch
    {
        KeyNotFoundException        => exception.Message,
        EmailNotVerifiedException   => exception.Message,
        UnauthorizedAccessException => exception.Message,
        ArgumentException           => exception.Message,
        InvalidOperationException   => exception.Message,
        SecurityTokenException      => exception.Message,
        _                           => UnexpectedErrorMessage,
    };
}

