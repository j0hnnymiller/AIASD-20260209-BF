namespace PostHubAPI.Exceptions;

/// <summary>
/// Exception thrown when a request contains invalid data or parameters
/// </summary>
public class BadRequestException : PostHubException
{
    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest, "BAD_REQUEST")
    {
    }
}
