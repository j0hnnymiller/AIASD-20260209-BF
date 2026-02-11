namespace PostHubAPI.Exceptions;

/// <summary>
/// Exception thrown when a user attempts an action they are not authorized to perform
/// </summary>
public class UnauthorizedException : PostHubException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, StatusCodes.Status403Forbidden, "UNAUTHORIZED")
    {
    }
}
