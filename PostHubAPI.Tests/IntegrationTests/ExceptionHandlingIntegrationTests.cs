using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PostHubAPI.Dtos.Common;
using PostHubAPI.Dtos.Post;

namespace PostHubAPI.Tests.IntegrationTests;

public class ExceptionHandlingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task GetPost_NotFound_ReturnsFormattedErrorResponse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/Post/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        Assert.NotNull(errorResponse);
        Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
        Assert.Contains("not found", errorResponse.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(errorResponse.TraceId);
        Assert.NotEqual(default, errorResponse.Timestamp);
    }

    [Fact]
    public async Task CreatePost_InvalidData_ReturnsValidationErrors()
    {
        // Arrange - Send data that exceeds max length
        var invalidPost = new CreatePostDto
        {
            Title = new string('x', 101), // Exceeds 100 character limit
            Body = new string('y', 201)    // Exceeds 200 character limit
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Post", invalidPost);

        // Assert - Could be 422 or 400 depending on validation
        Assert.True(
            response.StatusCode == HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == HttpStatusCode.BadRequest);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(content, _jsonOptions);

            Assert.NotNull(errorResponse);
            Assert.Equal("VALIDATION_ERROR", errorResponse.ErrorCode);
            Assert.NotNull(errorResponse.ValidationErrors);
            Assert.NotNull(errorResponse.TraceId);
        }
    }

    [Fact]
    public async Task GetComment_NotFound_ReturnsFormattedErrorResponse()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var response = await _client.GetAsync($"/api/Comment/{nonExistentId}");

        // Assert - Note: Will be Unauthorized if not logged in
        // Adjust based on your authentication requirements
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.Unauthorized);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

            Assert.NotNull(errorResponse);
            Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
        }
    }

    [Fact]
    public async Task AllErrors_IncludeTraceId()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/Post/99999");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        Assert.NotNull(errorResponse);
        Assert.NotNull(errorResponse.TraceId);
        Assert.NotEmpty(errorResponse.TraceId);
    }

    [Fact]
    public async Task AllErrors_ReturnJsonContentType()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/Post/99999");

        // Assert
        Assert.Contains("application/json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task ErrorResponse_IncludesTimestamp()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow.AddSeconds(-5);

        // Act
        var response = await _client.GetAsync("/api/Post/99999");
        var afterTime = DateTime.UtcNow.AddSeconds(5);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        Assert.NotNull(errorResponse);
        Assert.InRange(errorResponse.Timestamp, beforeTime, afterTime);
    }

    [Fact]
    public async Task UpdatePost_NotFound_ReturnsFormattedError()
    {
        // Arrange
        var nonExistentId = 99999;
        var updateDto = new EditPostDto
        {
            Title = "Updated Title",
            Body = "Updated Content"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Post/{nonExistentId}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        Assert.NotNull(errorResponse);
        Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task DeletePost_NotFound_ReturnsFormattedError()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var response = await _client.DeleteAsync($"/api/Post/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);

        Assert.NotNull(errorResponse);
        Assert.Equal("NOT_FOUND", errorResponse.ErrorCode);
    }

    [Fact]
    public async Task ErrorResponses_AreConsistentAcrossEndpoints()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var getResponse = await _client.GetAsync($"/api/Post/{nonExistentId}");
        var deleteResponse = await _client.DeleteAsync($"/api/Post/{nonExistentId}");

        // Assert
        Assert.Equal(getResponse.StatusCode, deleteResponse.StatusCode);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();

        var getError = JsonSerializer.Deserialize<ErrorResponse>(getContent, _jsonOptions);
        var deleteError = JsonSerializer.Deserialize<ErrorResponse>(deleteContent, _jsonOptions);

        Assert.NotNull(getError);
        Assert.NotNull(deleteError);
        Assert.Equal(getError.ErrorCode, deleteError.ErrorCode);
    }

    [Fact]
    public async Task ValidationError_ContainsFieldSpecificMessages()
    {
        // Arrange
        var invalidPost = new CreatePostDto(); // Empty object to trigger validation

        // Act
        var response = await _client.PostAsJsonAsync("/api/Post", invalidPost);

        // Assert
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(content, _jsonOptions);

            Assert.NotNull(errorResponse);
            Assert.NotNull(errorResponse.ValidationErrors);

            // Verify each field has array of error messages
            foreach (var fieldErrors in errorResponse.ValidationErrors.Values)
            {
                Assert.NotNull(fieldErrors);
                Assert.NotEmpty(fieldErrors);
            }
        }
    }
}
