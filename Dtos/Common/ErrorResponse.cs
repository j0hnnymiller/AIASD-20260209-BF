namespace PostHubAPI.Dtos.Common;

/// <summary>
/// Standard error response format for all API errors
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Machine-readable error code (e.g., "NOT_FOUND", "VALIDATION_ERROR")
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trace identifier for correlation with logs
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Optional additional contextual data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Error response format specifically for validation errors
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Dictionary mapping field names to their validation error messages
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; set; } = new();
}
