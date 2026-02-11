using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Tests.TestUtilities.Fixtures;

namespace PostHubAPI.Tests.IntegrationTests;

/// <summary>
/// Integration tests for Post API endpoints
/// Tests the full HTTP request/response cycle with real components
/// </summary>
[Trait("Category", "Integration")]
public class PostIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PostIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllPosts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/Post");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPosts_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/Post");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task CreatePost_WithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var createDto = new CreatePostDto
        {
            Title = "Integration Test Post",
            Body = "This is a test post created during integration testing"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Post", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/Post/");
    }

    [Fact]
    public async Task GetPostById_WhenPostExists_ReturnsPost()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "Test Post for Retrieval",
            Body = "This post will be retrieved by ID"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.GetAsync($"/api/Post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var post = await response.Content.ReadFromJsonAsync<ReadPostDto>();
        post.Should().NotBeNull();
        post!.Id.Should().Be(postId);
        post.Title.Should().Be("Test Post for Retrieval");
        post.Body.Should().Be("This post will be retrieved by ID");
    }

    [Fact]
    public async Task GetPostById_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/Post/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePost_WithValidData_ReturnsOkAndUpdatesPost()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "Original Title",
            Body = "Original Body"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        var updateDto = new EditPostDto
        {
            Title = "Updated Title",
            Body = "Updated Body"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Post/{postId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedPost = await response.Content.ReadFromJsonAsync<ReadPostDto>();
        updatedPost.Should().NotBeNull();
        updatedPost!.Title.Should().Be("Updated Title");
        updatedPost.Body.Should().Be("Updated Body");
    }

    [Fact]
    public async Task DeletePost_WhenPostExists_ReturnsNoContent()
    {
        // Arrange - Create a post first
        var createDto = new CreatePostDto
        {
            Title = "Post to Delete",
            Body = "This post will be deleted"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act
        var response = await _client.DeleteAsync($"/api/Post/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the post no longer exists
        var getResponse = await _client.GetAsync($"/api/Post/{postId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompletePostWorkflow_CreateReadUpdateDelete_Success()
    {
        // Step 1: Create
        var createDto = new CreatePostDto
        {
            Title = "Workflow Test Post",
            Body = "Testing complete CRUD workflow"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Post", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var postId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Step 2: Read
        var getResponse = await _client.GetAsync($"/api/Post/{postId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await getResponse.Content.ReadFromJsonAsync<ReadPostDto>();
        post!.Title.Should().Be("Workflow Test Post");

        // Step 3: Update
        var updateDto = new EditPostDto
        {
            Title = "Updated Workflow Post",
            Body = "Updated body content"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/Post/{postId}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Delete
        var deleteResponse = await _client.DeleteAsync($"/api/Post/{postId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 5: Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/Post/{postId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
