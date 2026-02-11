using PostHubAPI.Models;

namespace PostHubAPI.Tests.TestUtilities.Builders;

/// <summary>
/// Builder for creating Comment test data using the Fluent Builder pattern
/// </summary>
public class CommentBuilder
{
    private int _id = 1;
    private string _text = "Test comment";
    private int _postId = 1;
    private Post? _post = null;
    private DateTime? _createdAt = null;

    public CommentBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CommentBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    public CommentBuilder WithPostId(int postId)
    {
        _postId = postId;
        return this;
    }

    public CommentBuilder WithPost(Post post)
    {
        _post = post;
        _postId = post.Id;
        return this;
    }

    public CommentBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Comment Build()
    {
        return new Comment
        {
            Id = _id,
            Body = _text,
            PostId = _postId,
            Post = _post,
            CreationTime = _createdAt ?? DateTime.Now
        };
    }

    /// <summary>
    /// Creates a default Comment for quick testing
    /// </summary>
    public static Comment CreateDefault() => new CommentBuilder().Build();

    /// <summary>
    /// Creates a Comment with specified ID
    /// </summary>
    public static Comment CreateWithId(int id) => new CommentBuilder().WithId(id).Build();
}
