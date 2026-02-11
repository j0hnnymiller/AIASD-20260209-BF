using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PostHubAPI.Data;
using PostHubAPI.Dtos.Comment;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Exceptions;
using PostHubAPI.Models;
using PostHubAPI.Services.Implementations;
using PostHubAPI.Tests.TestUtilities.Builders;
using PostHubAPI.Tests.TestUtilities.Fixtures;

namespace PostHubAPI.Tests.UnitTests.Services;

/// <summary>
/// Unit tests for PostService
/// Tests business logic in isolation using mocked dependencies
/// </summary>
[Trait("Category", "Unit")]
public class PostServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PostService _sut; // System Under Test

    public PostServiceTests()
    {
        // Arrange - Set up test dependencies
        _context = DatabaseFixture.CreateInMemoryContext();
        _mockMapper = new Mock<IMapper>();
        _sut = new PostService(_context, _mockMapper.Object);
    }

    #region GetAllPostsAsync Tests

    [Fact]
    public async Task GetAllPostsAsync_WhenPostsExist_ReturnsAllPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new PostBuilder().WithId(1).WithTitle("Post 1").Build(),
            new PostBuilder().WithId(2).WithTitle("Post 2").Build(),
            new PostBuilder().WithId(3).WithTitle("Post 3").Build()
        };

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();

        var expectedDtos = new List<ReadPostDto>
        {
            new() { Id = 1, Title = "Post 1" },
            new() { Id = 2, Title = "Post 2" },
            new() { Id = 3, Title = "Post 3" }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<ReadPostDto>>(It.IsAny<List<Post>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetAllPostsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(expectedDtos);
        _mockMapper.Verify(m => m.Map<IEnumerable<ReadPostDto>>(It.IsAny<List<Post>>()), Times.Once);
    }

    [Fact]
    public async Task GetAllPostsAsync_WhenNoPostsExist_ReturnsEmptyCollection()
    {
        // Arrange
        _mockMapper.Setup(m => m.Map<IEnumerable<ReadPostDto>>(It.IsAny<List<Post>>()))
            .Returns(new List<ReadPostDto>());

        // Act
        var result = await _sut.GetAllPostsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPostsAsync_IncludesComments()
    {
        // Arrange
        var post = new PostBuilder()
            .WithId(1)
            .WithComment(new CommentBuilder().WithId(1).WithText("Comment 1").Build())
            .Build();

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<IEnumerable<ReadPostDto>>(It.IsAny<List<Post>>()))
            .Returns(new List<ReadPostDto> { new() { Id = 1, Comments = new List<ReadCommentDto> { new() { Id = 1, Body = "Comment 1" } } } });

        // Act
        var result = await _sut.GetAllPostsAsync();

        // Assert
        result.Should().NotBeNull();
        var postResult = result.First();
        postResult.Comments.Should().NotBeEmpty();
    }

    #endregion

    #region GetPostByIdAsync Tests

    [Fact]
    public async Task GetPostByIdAsync_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var post = new PostBuilder()
            .WithId(1)
            .WithTitle("Test Post")
            .WithBody("Test Body")
            .Build();

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var expectedDto = new ReadPostDto
        {
            Id = 1,
            Title = "Test Post",
            Body = "Test Body"
        };

        _mockMapper.Setup(m => m.Map<ReadPostDto>(It.IsAny<Post>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetPostByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        Func<Task> act = async () => await _sut.GetPostByIdAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post with ID 999 not found");
    }

    [Fact]
    public async Task GetPostByIdAsync_WhenPostHasComments_ReturnsPostWithComments()
    {
        // Arrange
        var comment = new CommentBuilder()
            .WithId(1)
            .WithText("Great post!")
            .Build();

        var post = new PostBuilder()
            .WithId(1)
            .WithComment(comment)
            .Build();

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var expectedDto = new ReadPostDto
        {
            Id = 1,
            Comments = new List<ReadCommentDto> { new() { Id = 1, Body = "Test comment" } }
        };

        _mockMapper.Setup(m => m.Map<ReadPostDto>(It.IsAny<Post>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetPostByIdAsync(1);

        // Assert
        result.Comments.Should().NotBeEmpty();
        result.Comments.Should().HaveCount(1);
    }

    #endregion

    #region CreateNewPostAsync Tests

    [Fact]
    public async Task CreateNewPostAsync_WithValidDto_ReturnsNewPostId()
    {
        // Arrange
        var createDto = new CreatePostDto
        {
            Title = "New Post",
            Body = "New Body"
        };

        var post = new PostBuilder()
            .WithTitle("New Post")
            .WithBody("New Body")
            .Build();

        _mockMapper.Setup(m => m.Map<Post>(createDto))
            .Returns(post);

        // Act
        var result = await _sut.CreateNewPostAsync(createDto);

        // Assert
        result.Should().BeGreaterThan(0);
        var createdPost = await _context.Posts.FirstOrDefaultAsync(p => p.Title == "New Post");
        createdPost.Should().NotBeNull();
        createdPost!.Body.Should().Be("New Body");
    }

    [Fact]
    public async Task CreateNewPostAsync_SavesPostToDatabase()
    {
        // Arrange
        var createDto = new CreatePostDto
        {
            Title = "Test",
            Body = "Test Body"
        };

        var post = new PostBuilder()
            .WithTitle("Test")
            .WithBody("Test Body")
            .Build();

        _mockMapper.Setup(m => m.Map<Post>(createDto))
            .Returns(post);

        var initialCount = await _context.Posts.CountAsync();

        // Act
        await _sut.CreateNewPostAsync(createDto);

        // Assert
        var finalCount = await _context.Posts.CountAsync();
        finalCount.Should().Be(initialCount + 1);
    }

    #endregion

    #region EditPostAsync Tests

    [Fact]
    public async Task EditPostAsync_WhenPostExists_UpdatesAndReturnsPost()
    {
        // Arrange
        var post = new PostBuilder()
            .WithId(1)
            .WithTitle("Original Title")
            .WithBody("Original Body")
            .Build();

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var editDto = new EditPostDto
        {
            Title = "Updated Title",
            Body = "Updated Body"
        };

        var expectedDto = new ReadPostDto
        {
            Id = 1,
            Title = "Updated Title",
            Body = "Updated Body"
        };

        _mockMapper.Setup(m => m.Map(editDto, It.IsAny<Post>()));
        _mockMapper.Setup(m => m.Map<ReadPostDto>(It.IsAny<Post>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.EditPostAsync(1, editDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        result.Body.Should().Be("Updated Body");
        _mockMapper.Verify(m => m.Map(editDto, It.IsAny<Post>()), Times.Once);
    }

    [Fact]
    public async Task EditPostAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var editDto = new EditPostDto
        {
            Title = "Updated",
            Body = "Updated"
        };

        // Act
        Func<Task> act = async () => await _sut.EditPostAsync(999, editDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post with ID 999 not found");
    }

    #endregion

    #region DeletePostAsync Tests

    [Fact]
    public async Task DeletePostAsync_WhenPostExists_RemovesPost()
    {
        // Arrange
        var post = new PostBuilder()
            .WithId(1)
            .WithTitle("To Delete")
            .Build();

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeletePostAsync(1);

        // Assert
        var deletedPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == 1);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task DeletePostAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        Func<Task> act = async () => await _sut.DeletePostAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post with ID 999 not found");
    }

    [Fact]
    public async Task DeletePostAsync_DecreasesPostCount()
    {
        // Arrange
        var post = new PostBuilder().WithId(1).Build();
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        var initialCount = await _context.Posts.CountAsync();

        // Act
        await _sut.DeletePostAsync(1);

        // Assert
        var finalCount = await _context.Posts.CountAsync();
        finalCount.Should().Be(initialCount - 1);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
