using PostHubAPI.Models;

namespace PostHubAPI.Tests.TestUtilities.Builders;

/// <summary>
/// Builder for creating Post test data using the Fluent Builder pattern
/// </summary>
public class PostBuilder
{
    private int _id = 1;
    private string _title = "Test Post";
    private string _body = "Test post body content";
    private DateTime _createdAt = DateTime.UtcNow;
    private List<Comment> _comments = new();

    public PostBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PostBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PostBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public PostBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public PostBuilder WithComments(List<Comment> comments)
    {
        _comments = comments;
        return this;
    }

    public PostBuilder WithComment(Comment comment)
    {
        _comments.Add(comment);
        return this;
    }

    public Post Build()
    {
        return new Post
        {
            Id = _id,
            Title = _title,
            Body = _body,
            Comments = _comments
        };
    }

    /// <summary>
    /// Creates a default Post for quick testing
    /// </summary>
    public static Post CreateDefault() => new PostBuilder().Build();

    /// <summary>
    /// Creates a Post with specified ID
    /// </summary>
    public static Post CreateWithId(int id) => new PostBuilder().WithId(id).Build();
}
