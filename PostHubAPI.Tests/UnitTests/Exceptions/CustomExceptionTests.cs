using Microsoft.AspNetCore.Http;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Tests.UnitTests.Exceptions;

public class CustomExceptionTests
{
    [Fact]
    public void NotFoundException_WithMessage_SetsCorrectProperties()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("NOT_FOUND", exception.ErrorCode);
    }

    [Fact]
    public void NotFoundException_WithResourceAndId_FormatsMessage()
    {
        // Arrange
        var resourceType = "Post";
        var id = 123;

        // Act
        var exception = new NotFoundException(resourceType, id);

        // Assert
        Assert.Equal("Post with ID 123 not found", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("NOT_FOUND", exception.ErrorCode);
        Assert.NotNull(exception.AdditionalData);
        Assert.Equal(resourceType, exception.AdditionalData["ResourceType"]);
        Assert.Equal(id, exception.AdditionalData["Id"]);
    }

    [Fact]
    public void NotFoundException_InheritsFromPostHubException()
    {
        // Arrange & Act
        var exception = new NotFoundException("Test");

        // Assert
        Assert.IsAssignableFrom<PostHubException>(exception);
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void UnauthorizedException_DefaultMessage_IsSet()
    {
        // Arrange & Act
        var exception = new UnauthorizedException();

        // Assert
        Assert.Equal("Unauthorized access", exception.Message);
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
        Assert.Equal("UNAUTHORIZED", exception.ErrorCode);
    }

    [Fact]
    public void UnauthorizedException_CustomMessage_IsPreserved()
    {
        // Arrange
        var message = "You don't have permission to access this resource";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact]
    public void BadRequestException_Message_IsPreserved()
    {
        // Arrange
        var message = "Invalid request format";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        Assert.Equal("BAD_REQUEST", exception.ErrorCode);
    }

    [Fact]
    public void ValidationException_WithErrors_StoresErrorDictionary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = new[] { "Email is required", "Email format is invalid" },
            ["Password"] = new[] { "Password must be at least 8 characters" }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal("One or more validation errors occurred", exception.Message);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, exception.StatusCode);
        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        Assert.Equal(errors, exception.ValidationErrors);
        Assert.NotNull(exception.AdditionalData);
        Assert.Contains("Errors", exception.AdditionalData.Keys);
    }

    [Fact]
    public void PostHubException_AdditionalData_CanBeSet()
    {
        // Arrange
        var exception = new NotFoundException("Test");
        var additionalData = new Dictionary<string, object>
        {
            ["CustomKey"] = "CustomValue",
            ["Timestamp"] = DateTime.UtcNow
        };

        // Act
        exception.AdditionalData = additionalData;

        // Assert
        Assert.NotNull(exception.AdditionalData);
        Assert.Equal("CustomValue", exception.AdditionalData["CustomKey"]);
    }

    [Fact]
    public void AllCustomExceptions_HaveCorrectStatusCodes()
    {
        // Arrange & Act
        var notFound = new NotFoundException("Test");
        var unauthorized = new UnauthorizedException();
        var badRequest = new BadRequestException("Test");
        var validation = new ValidationException(new Dictionary<string, string[]>());

        // Assert
        Assert.Equal(404, notFound.StatusCode);
        Assert.Equal(403, unauthorized.StatusCode);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal(422, validation.StatusCode);
    }

    [Fact]
    public void AllCustomExceptions_HaveCorrectErrorCodes()
    {
        // Arrange & Act
        var notFound = new NotFoundException("Test");
        var unauthorized = new UnauthorizedException();
        var badRequest = new BadRequestException("Test");
        var validation = new ValidationException(new Dictionary<string, string[]>());

        // Assert
        Assert.Equal("NOT_FOUND", notFound.ErrorCode);
        Assert.Equal("UNAUTHORIZED", unauthorized.ErrorCode);
        Assert.Equal("BAD_REQUEST", badRequest.ErrorCode);
        Assert.Equal("VALIDATION_ERROR", validation.ErrorCode);
    }
}
