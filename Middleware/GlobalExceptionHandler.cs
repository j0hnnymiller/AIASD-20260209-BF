using Microsoft.AspNetCore.Diagnostics;
using PostHubAPI.Dtos.Common;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Middleware;

/// <summary>
/// Global exception handler that catches all unhandled exceptions
/// and returns consistent error responses
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = context.TraceIdentifier;

        // Log with appropriate level based on exception type
        LogException(exception, traceId);

        // Build response based on exception type
        var (statusCode, errorResponse) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ValidationErrorResponse
                {
                    ErrorCode = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred",
                    TraceId = traceId,
                    ValidationErrors = validationEx.ValidationErrors
                }
            ),
            PostHubException customEx => (
                customEx.StatusCode,
                new ErrorResponse
                {
                    ErrorCode = customEx.ErrorCode,
                    Message = customEx.Message,
                    TraceId = traceId,
                    AdditionalData = customEx.AdditionalData
                }
            ),
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    ErrorCode = "BAD_REQUEST",
                    Message = argEx.Message,
                    TraceId = traceId
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later.",
                    TraceId = traceId,
                    AdditionalData = _environment.IsDevelopment()
                        ? new Dictionary<string, object> { ["StackTrace"] = exception.StackTrace ?? "" }
                        : null
                }
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true; // Exception handled
    }

    private void LogException(Exception exception, string traceId)
    {
        var logLevel = exception switch
        {
            NotFoundException => LogLevel.Warning,
            UnauthorizedException => LogLevel.Warning,
            BadRequestException => LogLevel.Warning,
            ValidationException => LogLevel.Information,
            ArgumentException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception occurred. TraceId: {TraceId}, ExceptionType: {ExceptionType}",
            traceId,
            exception.GetType().Name);
    }
}
