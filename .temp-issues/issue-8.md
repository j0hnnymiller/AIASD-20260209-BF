## Severity: MEDIUM

### Description

Using `DateTime.Now` instead of `DateTime.UtcNow` creates timezone issues and makes code non-testable.

### Current Code

```csharp
// Post.cs line 10
public DateTime CreationTime { get; } = DateTime.Now;

// Comment.cs line 9
public DateTime CreationTime { get; set; } = DateTime.Now;

// UserService.cs line 74
expires: DateTime.Now.AddHours(3)
```

### Impact

- Timezone inconsistencies in distributed systems
- Non-testable (static dependency)
- Daylight saving time issues
- Difficult to handle users in different timezones

### Recommended Fix

1. Use `DateTime.UtcNow` everywhere
2. Better: Inject time abstraction:

```csharp
public interface ISystemClock
{
    DateTime UtcNow { get; }
}

public class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
```

3. Update models to not set default values (set in service layer)

### Files Affected

- `Models/Post.cs`
- `Models/Comment.cs`
- `Services/Implementations/UserService.cs`
- New: `Services/ISystemClock.cs`
- `Program.cs` (service registration)

### Priority

🟡 Medium - Architecture Issue
