## Severity: MEDIUM

### Description

No structured logging implemented throughout the application, making debugging and monitoring difficult.

### Impact

- Difficult to troubleshoot production issues
- No observability into application behavior
- Cannot track performance or errors effectively

### Recommended Fix

1. Inject `ILogger<T>` into services and controllers
2. Add structured logging at key points:

```csharp
public class PostService
{
    private readonly ILogger<PostService> _logger;

    public PostService(ApplicationDbContext context, IMapper mapper, ILogger<PostService> logger)
    {
        _logger = logger;
    }

    public async Task<ReadPostDto> GetPostByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving post with ID {PostId}", id);

        var post = await context.Posts.Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            _logger.LogWarning("Post with ID {PostId} not found", id);
            throw new NotFoundException("Post not found!");
        }

        _logger.LogInformation("Successfully retrieved post {PostId}", id);
        return mapper.Map<ReadPostDto>(post);
    }
}
```

3. Configure logging providers in appsettings.json
4. Consider Application Insights or Serilog for production

### Files Affected

- All services
- All controllers
- `Program.cs` (logging configuration)
- `appsettings.json`

### Priority

🟡 Medium - Observability
