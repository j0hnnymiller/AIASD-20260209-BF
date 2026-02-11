namespace PostHubAPI.Exceptions;

/// <summary>
/// Base class for all custom PostHub exceptions.
/// Provides consistent error handling with status codes and error codes.
/// </summary>
public abstract class PostHubException : Exception
{
    /// <summary>
    /// HTTP status code to return for this exception
    /// </summary>
    public int StatusCode { get; protected set; }

    /// <summary>
    /// Machine-readable error code for API consumers
    /// </summary>
    public string ErrorCode { get; protected set; }

    /// <summary>
    /// Optional additional data to include in error response
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    protected PostHubException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
