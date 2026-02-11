## Severity: MEDIUM

### Description

All service methods duplicate the same null-checking pattern when retrieving entities. This creates code duplication and maintenance overhead.

### Current Pattern

```csharp
Post? post = await context.Posts.FirstOrDefaultAsync(...);
if (post != null) {
    // logic
} else {
    throw new NotFoundException("Post not found!");
}
```

### Recommended Fix

Create generic extension method:

```csharp
public static class QueryableExtensions
{
    public static async Task<T> GetOrThrowAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        string? errorMessage = null) where T : class
    {
        var entity = await query.FirstOrDefaultAsync(predicate);
        if (entity == null)
        {
            throw new NotFoundException(
                errorMessage ?? $"{typeof(T).Name} not found!");
        }
        return entity;
    }
}
```

Usage:

```csharp
Post post = await context.Posts.GetOrThrowAsync(p => p.Id == id);
// No null check needed - exception is thrown automatically
```

### Files Affected

- New: `Extensions/QueryableExtensions.cs`
- `Services/Implementations/PostService.cs`
- `Services/Implementations/CommentService.cs`

### Priority

🟡 Medium - Code Quality
