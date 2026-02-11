## Severity: MEDIUM

### Description

Eager loading all comments for all posts can cause performance issues with N+1 query pattern.

### Current Code

```csharp
// PostService.cs line 14
List<Post> posts = await context.Posts.Include(p => p.Comments).ToListAsync();
```

### Impact

- Loads unnecessary data when user only needs post summaries
- Large payload over network
- Slow query execution with many comments
- Memory overhead

### Recommended Fix

Option 1: Separate endpoints

```csharp
// For list view - no comments
public async Task<IEnumerable<ReadPostDto>> GetAllPostsAsync()
{
    var posts = await context.Posts.ToListAsync();
    return mapper.Map<IEnumerable<ReadPostDto>>(posts);
}

// For detail view - with comments
public async Task<ReadPostDetailDto> GetPostByIdAsync(int id)
{
    var post = await context.Posts
        .Include(p => p.Comments)
        .FirstOrDefaultAsync(p => p.Id == id);
    return mapper.Map<ReadPostDetailDto>(post);
}
```

Option 2: Use projection

```csharp
var posts = await context.Posts
    .Select(p => new ReadPostDto
    {
        Id = p.Id,
        Title = p.Title,
        Body = p.Body,
        CreationTime = p.CreationTime,
        Likes = p.Likes,
        CommentCount = p.Comments.Count
    })
    .ToListAsync();
```

Option 3: Split queries

```csharp
var posts = await context.Posts
    .Include(p => p.Comments)
    .AsSplitQuery()
    .ToListAsync();
```

### Files Affected

- `Services/Implementations/PostService.cs`
- `Dtos/Post/ReadPostDto.cs` (add CommentCount)

### Priority

🟡 Medium - Performance Optimization
