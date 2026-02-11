## Severity: HIGH

### Description

`GetAllPosts` endpoint loads all posts without pagination, which will cause performance and memory issues as data grows.

### Current Code

```csharp
// PostService.cs line 14
public async Task<IEnumerable<ReadPostDto>> GetAllPostsAsync()
{
    List<Post> posts = await context.Posts.Include(p => p.Comments).ToListAsync();
    return mapper.Map<IEnumerable<ReadPostDto>>(posts);
}
```

### Impact

- Memory exhaustion with large datasets
- Slow response times
- Network bandwidth waste
- Poor user experience

### Recommended Fix

Add pagination parameters:

```csharp
public async Task<PagedResult<ReadPostDto>> GetAllPostsAsync(
    int pageNumber = 1,
    int pageSize = 10)
{
    var totalCount = await context.Posts.CountAsync();

    var posts = await context.Posts
        .Include(p => p.Comments)
        .OrderByDescending(p => p.CreationTime)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var items = mapper.Map<IEnumerable<ReadPostDto>>(posts);

    return new PagedResult<ReadPostDto>
    {
        Items = items,
        PageNumber = pageNumber,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

### Files Affected

- `Services/Interfaces/IPostService.cs`
- `Services/Implementations/PostService.cs`
- `Controllers/PostController.cs`
- New: `Models/PagedResult.cs`

### Priority

🟠 High - Performance Issue
