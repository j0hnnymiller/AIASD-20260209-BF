namespace PostHubAPI.Exceptions;

/// <summary>
/// Exception thrown when model validation fails
/// </summary>
public class ValidationException : PostHubException
{
    /// <summary>
    /// Dictionary of field names to validation error messages
    /// </summary>
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred",
               StatusCodes.Status422UnprocessableEntity,
               "VALIDATION_ERROR")
    {
        ValidationErrors = errors;
        AdditionalData = new Dictionary<string, object> { ["Errors"] = errors };
    }
}
