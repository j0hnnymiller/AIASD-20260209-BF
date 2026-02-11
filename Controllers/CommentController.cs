using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostHubAPI.Dtos.Comment;
using PostHubAPI.Exceptions;
using PostHubAPI.Services.Interfaces;

namespace PostHubAPI.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class CommentController(ICommentService commentService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(int id)
    {
        var comment = await commentService.GetCommentAsync(id);
        return Ok(comment);
    }

    [HttpPost("{postId}")]
    public async Task<IActionResult> CreateNewComment(int postId, [FromBody]CreateCommentDto dto)
    {
        var newCommentId = await commentService.CreateNewCommnentAsync(postId, dto);
        var locationUri = $"{Request.Scheme}://{Request.Host}/api/Comment/{newCommentId}";
        return Created(locationUri, newCommentId);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditComment(int id, [FromBody]EditCommentDto dto)
    {
        var editedComment = await commentService.EditCommentAsync(id, dto);
        return Ok(editedComment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        await commentService.DeleteCommentAsync(id);
        return NoContent();
    }
}