namespace PostHubAPI.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found
/// </summary>
public class NotFoundException : PostHubException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound, "NOT_FOUND")
    {
    }

    public NotFoundException(string resourceType, int id)
        : base($"{resourceType} with ID {id} not found",
               StatusCodes.Status404NotFound,
               "NOT_FOUND")
    {
        AdditionalData = new Dictionary<string, object>
        {
            ["ResourceType"] = resourceType,
            ["Id"] = id
        };
    }
}