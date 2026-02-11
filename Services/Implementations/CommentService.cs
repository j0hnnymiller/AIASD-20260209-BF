using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Data;
using PostHubAPI.Dtos.Comment;
using PostHubAPI.Exceptions;
using PostHubAPI.Extensions;
using PostHubAPI.Models;
using PostHubAPI.Services.Interfaces;

namespace PostHubAPI.Services.Implementations;

public class CommentService(ApplicationDbContext context, IMapper mapper) : ICommentService
{
    public async Task<ReadCommentDto> GetCommentAsync(int id)
    {
        Comment comment = await context.Comments.GetOrThrowAsync(c => c.Id == id, id);

        return mapper.Map<ReadCommentDto>(comment);
    }

    public async Task<int> CreateNewCommnentAsync(int postId, CreateCommentDto newComment)
    {
        Post post = await context.Posts.GetOrThrowAsync(c => c.Id == postId, postId);

        Comment comment = mapper.Map<Comment>(newComment);
        comment.Post = post;
        comment.PostId = postId;
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        return comment.Id;
    }

    public async Task<ReadCommentDto> EditCommentAsync(int id, EditCommentDto dto)
    {
        Comment commentToEdit = await context.Comments.GetOrThrowAsync(comment => comment.Id == id, id);

        mapper.Map(dto, commentToEdit);
        await context.SaveChangesAsync();

        return mapper.Map<ReadCommentDto>(commentToEdit);
    }

    public async Task DeleteCommentAsync(int id)
    {
        Comment comment = await context.Comments.GetOrThrowAsync(comment => comment.Id == id, id);

        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
    }
}
