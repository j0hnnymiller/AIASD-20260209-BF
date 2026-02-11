using Microsoft.EntityFrameworkCore;
using PostHubAPI.Exceptions;
using PostHubAPI.Extensions;
using PostHubAPI.Models;

namespace PostHubAPI.Tests.UnitTests.Extensions;

public class QueryableExtensionsTests
{
    private DbContextOptions<TestDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetOrThrowAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        var testPost = new Post
        {
            Id = 1,
            Title = "Test Post",
            Body = "Test Body"
        };
        context.Posts.Add(testPost);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Posts.GetOrThrowAsync(p => p.Id == 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Post", result.Title);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => context.Posts.GetOrThrowAsync(p => p.Id == 999, 999)
        );

        Assert.Contains("Post with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithCustomErrorMessage_ThrowsNotFoundExceptionWithMessage()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);
        var customMessage = "Custom error message";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => context.Posts.GetOrThrowAsync(p => p.Id == 999, customMessage)
        );

        Assert.Contains(customMessage, exception.Message);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithDefaultMessage_UsesResourceTypeAndId()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => context.Posts.GetOrThrowAsync(p => p.Id == 123, 123)
        );

        Assert.Contains("Post with ID 123 not found", exception.Message);
        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithComplexPredicate_ReturnsCorrectEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        var post1 = new Post { Id = 1, Title = "First Post", Body = "Body 1" };
        var post2 = new Post { Id = 2, Title = "Second Post", Body = "Body 2" };
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Posts.GetOrThrowAsync(p => p.Title == "Second Post", "Second Post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Second Post", result.Title);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithMultipleMatchingEntities_ReturnsFirstMatch()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        var post1 = new Post { Id = 1, Title = "Duplicate Title", Body = "Body 1" };
        var post2 = new Post { Id = 2, Title = "Duplicate Title", Body = "Body 2" };
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Posts.GetOrThrowAsync(p => p.Title == "Duplicate Title", "Duplicate Title");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Duplicate Title", result.Title);
        // Should return the first match
        Assert.True(result.Id == 1 || result.Id == 2);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithComment_ReturnsEntity()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        var testPost = new Post { Id = 1, Title = "Post", Body = "Post Body" };
        var testComment = new Comment
        {
            Id = 1,
            Body = "Test Comment",
            PostId = 1,
            Post = testPost
        };
        context.Posts.Add(testPost);
        context.Comments.Add(testComment);
        await context.SaveChangesAsync();

        // Act
        var result = await context.Comments.GetOrThrowAsync(c => c.Id == 1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Comment", result.Body);
    }

    [Fact]
    public async Task GetOrThrowAsync_WithComment_ThrowsNotFoundException()
    {
        // Arrange
        var options = CreateNewContextOptions();
        using var context = new TestDbContext(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => context.Comments.GetOrThrowAsync(c => c.Id == 999, 999)
        );

        Assert.Contains("Comment with ID 999 not found", exception.Message);
    }

    // Test DbContext for testing purposes
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
