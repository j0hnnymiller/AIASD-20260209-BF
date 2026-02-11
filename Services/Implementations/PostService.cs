using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Data;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Exceptions;
using PostHubAPI.Extensions;
using PostHubAPI.Models;
using PostHubAPI.Services.Interfaces;

namespace PostHubAPI.Services.Implementations;

public class PostService(ApplicationDbContext context, IMapper mapper) : IPostService
{
    public async Task<IEnumerable<ReadPostDto>> GetAllPostsAsync()
    {
        List<Post> posts = await context.Posts.Include(p => p.Comments).ToListAsync();
        return mapper.Map<IEnumerable<ReadPostDto>>(posts);
    }

    public async Task<ReadPostDto> GetPostByIdAsync(int id)
    {
        Post post = await context.Posts.Include(p => p.Comments).GetOrThrowAsync(p => p.Id == id, "Post not found!");
        ReadPostDto dto = mapper.Map<ReadPostDto>(post);
        return dto;
    }

    public async Task<int> CreateNewPostAsync(CreatePostDto dto)
    {
        Post post = mapper.Map<Post>(dto);
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post.Id;
    }

    public async Task<ReadPostDto> EditPostAsync(int id, EditPostDto dto)
    {
        Post postToEdit = await context.Posts.GetOrThrowAsync(p => p.Id == id, "Post not found!");
        mapper.Map(dto, postToEdit);
        await context.SaveChangesAsync();

        ReadPostDto readPost = mapper.Map<ReadPostDto>(postToEdit);
        return readPost;
    }

    public async Task DeletePostAsync(int id)
    {
        Post post = await context.Posts.GetOrThrowAsync(p => p.Id == id, "Post not found!");
        context.Posts.Remove(post);
        await context.SaveChangesAsync();
    }
}