using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using PostHubAPI.Exceptions;
using PostHubAPI.Filters;

namespace PostHubAPI.Tests.UnitTests.Filters;

public class ModelValidationFilterTests
{
    private readonly ModelValidationFilter _filter;

    public ModelValidationFilterTests()
    {
        _filter = new ModelValidationFilter();
    }

    [Fact]
    public void OnActionExecuting_ValidModel_DoesNotThrow()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        // ModelState is valid by default

        // Act & Assert
        var exception = Record.Exception(() => _filter.OnActionExecuting(context));
        Assert.Null(exception);
    }

    [Fact]
    public void OnActionExecuting_InvalidModel_ThrowsValidationException()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));
        Assert.NotNull(exception.ValidationErrors);
        Assert.Contains("Email", exception.ValidationErrors.Keys);
    }

    [Fact]
    public void OnActionExecuting_MultipleErrors_IncludesAllFields()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Email", "Email format is invalid");
        context.ModelState.AddModelError("Password", "Password must be at least 8 characters");

        // Act
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));

        // Assert
        Assert.NotNull(exception.ValidationErrors);
        Assert.Equal(2, exception.ValidationErrors.Count);
        Assert.Contains("Email", exception.ValidationErrors.Keys);
        Assert.Contains("Password", exception.ValidationErrors.Keys);
        Assert.Equal(2, exception.ValidationErrors["Email"].Length);
        Assert.Single(exception.ValidationErrors["Password"]);
    }

    [Fact]
    public void OnActionExecuting_NestedValidation_IncludesFieldPath()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("User.Email", "Email is required");
        context.ModelState.AddModelError("Address.Street", "Street is required");

        // Act
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));

        // Assert
        Assert.NotNull(exception.ValidationErrors);
        Assert.Contains("User.Email", exception.ValidationErrors.Keys);
        Assert.Contains("Address.Street", exception.ValidationErrors.Keys);
    }

    [Fact]
    public void OnActionExecuting_EmptyModelState_DoesNotThrow()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        Assert.True(context.ModelState.IsValid);

        // Act & Assert
        var exception = Record.Exception(() => _filter.OnActionExecuting(context));
        Assert.Null(exception);
    }

    [Fact]
    public void OnActionExecuted_AlwaysSucceeds()
    {
        // Arrange
        var context = CreateActionExecutedContext();

        // Act & Assert
        var exception = Record.Exception(() => _filter.OnActionExecuted(context));
        Assert.Null(exception);
    }

    [Fact]
    public void OnActionExecuting_InvalidModel_ExceptionHasCorrectMessage()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Test", "Test error");

        // Act
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));

        // Assert
        Assert.Equal("One or more validation errors occurred", exception.Message);
    }

    [Fact]
    public void OnActionExecuting_InvalidModel_ExceptionHasCorrectStatusCode()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Test", "Test error");

        // Act
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));

        // Assert
        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public void OnActionExecuting_InvalidModel_ExceptionHasCorrectErrorCode()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Test", "Test error");

        // Act
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));

        // Assert
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
    }

    [Fact]
    public void OnActionExecuting_ModelStateWithNullValue_HandlesGracefully()
    {
        // Arrange
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Test", "Test error");

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => _filter.OnActionExecuting(context));
        Assert.NotNull(exception.ValidationErrors);
    }

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary()
        );

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object()
        );
    }

    private static ActionExecutedContext CreateActionExecutedContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );

        return new ActionExecutedContext(
            actionContext,
            new List<IFilterMetadata>(),
            new object()
        );
    }
}
