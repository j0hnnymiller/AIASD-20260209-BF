using Microsoft.AspNetCore.Mvc;
using PostHubAPI.Dtos.Post;
using PostHubAPI.Exceptions;
using PostHubAPI.Services.Interfaces;

namespace PostHubAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController(IPostService postService) : ControllerBase
{
    private readonly IPostService _postService = postService;

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostById(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
    {
        var newPostId = await _postService.CreateNewPostAsync(dto);
        var locationUri = $"{Request.Scheme}://{Request.Host}/api/Post/{newPostId}";
        return Created(locationUri, newPostId);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditPost(int id, EditPostDto dto)
    {
        var editedPost = await _postService.EditPostAsync(id, dto);
        return Ok(editedPost);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _postService.DeletePostAsync(id);
        return NoContent();
    }
}