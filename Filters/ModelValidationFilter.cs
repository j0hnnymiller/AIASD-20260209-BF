using Microsoft.AspNetCore.Mvc.Filters;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Filters;

/// <summary>
/// Action filter that automatically validates model state and throws ValidationException
/// if validation fails. This eliminates the need for manual ModelState.IsValid checks
/// in controllers.
/// </summary>
public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value != null && x.Value.Errors.Any())
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
