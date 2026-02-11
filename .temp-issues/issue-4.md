## Severity: HIGH

### Description

Post and Comment controllers lack authorization checks. Any authenticated user can edit or delete any post/comment, regardless of ownership.

### Current Issues

1. `PostController` - No `[Authorize]` attribute, no ownership validation
2. `CommentController` - Has `[Authorize]` but no ownership validation
3. Anyone can delete/edit content they didn't create

### Impact

- Users can modify or delete other users' content
- No access control enforcement
- Potential for malicious content manipulation

### Recommended Fix

1. Add `[Authorize]` to PostController
2. Add ownership validation in service layer:

```csharp
public async Task<ReadPostDto> EditPostAsync(int id, EditPostDto dto, string userId)
{
    Post? post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
    if (post == null) throw new NotFoundException("Post not found!");

    if (post.UserId != userId)
        throw new UnauthorizedException("You can only edit your own posts!");

    // ... rest of logic
}
```

3. Add UserId to Post and Comment models
4. Extract user ID from claims in controllers

### Files Affected

- `Controllers/PostController.cs`
- `Controllers/CommentController.cs`
- `Services/Implementations/PostService.cs`
- `Services/Implementations/CommentService.cs`
- `Models/Post.cs`
- `Models/Comment.cs`

### Priority

🟠 High - Authorization Issue
