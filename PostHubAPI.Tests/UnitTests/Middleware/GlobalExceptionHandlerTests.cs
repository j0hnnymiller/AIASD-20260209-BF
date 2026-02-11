using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PostHubAPI.Dtos.Common;
using PostHubAPI.Exceptions;
using PostHubAPI.Middleware;

namespace PostHubAPI.Tests.UnitTests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object, _environmentMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithCorrectFormat()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Post", 123);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(404, context.Response.StatusCode);
        Assert.Contains("application/json", context.Response.ContentType);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
        Assert.Equal("Post with ID 123 not found", errorResponse.Message);
        Assert.NotNull(errorResponse.TraceId);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns422WithErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = new[] { "Email is required" },
            ["Password"] = new[] { "Password must be at least 8 characters" }
        };
        var exception = new ValidationException(errors);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(422, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        Assert.Contains("VALIDATION_ERROR", responseBody);
        Assert.Contains("validation errors occurred", responseBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedException("Access denied");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(403, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("UNAUTHORIZED", errorResponse.ErrorCode);
        Assert.Equal("Access denied", errorResponse.Message);
    }

    [Fact]
    public async Task TryHandleAsync_BadRequestException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new BadRequestException("Invalid input");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(400, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("BAD_REQUEST", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400WithMessage()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument provided");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(400, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("BAD_REQUEST", errorResponse.ErrorCode);
        Assert.Equal("Invalid argument provided", errorResponse.Message);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500InProduction()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("INTERNAL_SERVER_ERROR", errorResponse.ErrorCode);
        Assert.Equal("An unexpected error occurred. Please try again later.", errorResponse.Message);
        Assert.Null(errorResponse.AdditionalData); // No stack trace in production
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_IncludesStackTraceInDevelopment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("Something went wrong", errorResponse.Message); // Shows actual message in dev
        Assert.NotNull(errorResponse.AdditionalData); // Stack trace included
    }

    [Fact]
    public async Task TryHandleAsync_AllExceptions_IncludeTraceId()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.TraceId);
        Assert.NotEmpty(errorResponse.TraceId);
    }

    [Fact]
    public async Task TryHandleAsync_AllExceptions_IncludeTimestamp()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test");

        // Act
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.InRange(errorResponse.Timestamp, beforeTime, afterTime);
    }

    [Fact]
    public async Task TryHandleAsync_CustomException_IncludesAdditionalData()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Post", 123);

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        var responseBody = GetResponseBody(context);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.AdditionalData);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_LogsAsWarning()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Test");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_LogsAsError()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_LogsAsInformation()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ValidationException(new Dictionary<string, string[]>());

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = Guid.NewGuid().ToString();
        return context;
    }

    private static string GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return reader.ReadToEnd();
    }
}
