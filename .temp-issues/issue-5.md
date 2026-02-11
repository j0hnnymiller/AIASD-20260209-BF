## Severity: MEDIUM

### Description

Repetitive try-catch exception handling pattern duplicated across all controller methods. This violates DRY principle and makes maintenance difficult.

### Current Pattern

```csharp
try {
    // logic
} catch (NotFoundException exception) {
    return NotFound(exception.Message);
}
```

### Impact

- Code duplication across controllers
- Difficult to maintain consistent error responses
- Hard to add logging or telemetry

### Recommended Fix

Implement global exception handling middleware:

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var response = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedException => (StatusCodes.Status403Forbidden, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred")
        };

        context.Response.StatusCode = response.Item1;
        await context.Response.WriteAsJsonAsync(new { error = response.Item2 }, cancellationToken);
        return true;
    }
}
```

Register in Program.cs:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

Then remove all try-catch blocks from controllers.

### Files Affected

- All controllers
- New: `Middleware/GlobalExceptionHandler.cs`
- `Program.cs`

### Priority

🟡 Medium - Code Quality

---

## 📋 Detailed Implementation Proposal

### Phase 1: Create Exception Custom Types (1-2 hours)

**Step 1.1: Create Base Exception Class**

- **File**: `Exceptions/PostHubException.cs`
- **Purpose**: Base class for all custom exceptions with common properties

```csharp
namespace PostHubAPI.Exceptions;

public abstract class PostHubException : Exception
{
    public int StatusCode { get; protected set; }
    public string ErrorCode { get; protected set; }
    public Dictionary<string, object>? AdditionalData { get; set; }

    protected PostHubException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
```

**Step 1.2: Create Specific Exception Types**

- **File**: `Exceptions/NotFoundException.cs` (update existing)
- **File**: `Exceptions/UnauthorizedException.cs` (new)
- **File**: `Exceptions/BadRequestException.cs` (new)
- **File**: `Exceptions/ValidationException.cs` (new)

```csharp
// NotFoundException.cs
public class NotFoundException : PostHubException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound, "NOT_FOUND") { }

    public NotFoundException(string resourceType, int id)
        : base($"{resourceType} with ID {id} not found",
               StatusCodes.Status404NotFound,
               "NOT_FOUND")
    {
        AdditionalData = new() { ["ResourceType"] = resourceType, ["Id"] = id };
    }
}

// UnauthorizedException.cs
public class UnauthorizedException : PostHubException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, StatusCodes.Status403Forbidden, "UNAUTHORIZED") { }
}

// BadRequestException.cs
public class BadRequestException : PostHubException
{
    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest, "BAD_REQUEST") { }
}

// ValidationException.cs
public class ValidationException : PostHubException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred",
               StatusCodes.Status422UnprocessableEntity,
               "VALIDATION_ERROR")
    {
        ValidationErrors = errors;
        AdditionalData = new() { ["Errors"] = errors };
    }
}
```

### Phase 2: Create Error Response Models (30 minutes)

**Step 2.1: Create Standard Error Response DTO**

- **File**: `Dtos/Common/ErrorResponse.cs`

```csharp
namespace PostHubAPI.Dtos.Common;

public class ErrorResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> ValidationErrors { get; set; }
}
```

### Phase 3: Implement Global Exception Handler (2-3 hours)

**Step 3.1: Create Exception Handler**

- **File**: `Middleware/GlobalExceptionHandler.cs`

```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PostHubAPI.Dtos.Common;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = context.TraceIdentifier;

        // Log with appropriate level based on exception type
        LogException(exception, traceId);

        // Build response based on exception type
        var (statusCode, errorResponse) = exception switch
        {
            PostHubException customEx => (
                customEx.StatusCode,
                new ErrorResponse
                {
                    ErrorCode = customEx.ErrorCode,
                    Message = customEx.Message,
                    TraceId = traceId,
                    AdditionalData = customEx.AdditionalData
                }
            ),
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ValidationErrorResponse
                {
                    ErrorCode = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred",
                    TraceId = traceId,
                    ValidationErrors = validationEx.ValidationErrors
                }
            ),
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse
                {
                    ErrorCode = "BAD_REQUEST",
                    Message = argEx.Message,
                    TraceId = traceId
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later.",
                    TraceId = traceId,
                    AdditionalData = _environment.IsDevelopment()
                        ? new Dictionary<string, object> { ["StackTrace"] = exception.StackTrace ?? "" }
                        : null
                }
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true; // Exception handled
    }

    private void LogException(Exception exception, string traceId)
    {
        var logLevel = exception switch
        {
            NotFoundException => LogLevel.Warning,
            UnauthorizedException => LogLevel.Warning,
            BadRequestException => LogLevel.Warning,
            ValidationException => LogLevel.Information,
            ArgumentException => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception occurred. TraceId: {TraceId}, ExceptionType: {ExceptionType}",
            traceId,
            exception.GetType().Name);
    }
}
```

**Step 3.2: Register Exception Handler**

- **File**: `Program.cs` (update)
- **Location**: After `builder.Services.AddEndpointsApiExplorer();`

```csharp
// Add exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // For standard problem details format
```

- **Location**: After `app.UseHttpsRedirection();`

```csharp
// Must be before UseAuthentication/UseAuthorization
app.UseExceptionHandler();
```

### Phase 4: Update Controllers (1-2 hours)

**Step 4.1: Remove Try-Catch Blocks**

For each controller, remove try-catch blocks and simplify methods:

**Before** (PostController.cs):

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetPostById(int id)
{
    try
    {
        var post = await _postService.GetPostByIdAsync(id);
        return Ok(post);
    }
    catch (NotFoundException exception)
    {
        return NotFound(exception.Message);
    }
}
```

**After**:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetPostById(int id)
{
    var post = await _postService.GetPostByIdAsync(id);
    return Ok(post);
}
```

**Files to Update**:

- `Controllers/PostController.cs` (5 methods)
- `Controllers/CommentController.cs` (4 methods)
- `Controllers/UserController.cs` (2 methods)

**Step 4.2: Update Service Exception Throwing**

Services can remain unchanged, but consider adding more specific exception types:

```csharp
// Instead of:
throw new NotFoundException("Post not found!");

// Use:
throw new NotFoundException("Post", id);
```

### Phase 5: Add Model Validation Filter (1 hour)

**Step 5.1: Create Validation Filter**

- **File**: `Filters/ModelValidationFilter.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Filters;

public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
```

**Step 5.2: Register Filter**

- **File**: `Program.cs`

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationFilter>();
});
```

**Step 5.3: Remove Manual ModelState Validation**

Remove these blocks from all controllers:

```csharp
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

### Phase 6: Testing Strategy (2-3 hours)

**Step 6.1: Unit Tests for Exception Handler**

- **File**: `PostHubAPI.Tests/UnitTests/Middleware/GlobalExceptionHandlerTests.cs`

```csharp
public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_NotFoundException_Returns404WithCorrectFormat()
    {
        // Arrange
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new NotFoundException("Post", 123);

        // Act
        var result = await handler.TryHandleAsync(context, exception, default);

        // Assert
        Assert.True(result);
        Assert.Equal(404, context.Response.StatusCode);
        // Verify response body structure
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns422WithErrors()
    {
        // Test validation error handling
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_Returns500InProduction()
    {
        // Test generic exception handling
    }
}
```

**Step 6.2: Integration Tests**

- **File**: `PostHubAPI.Tests/IntegrationTests/ExceptionHandlingTests.cs`

```csharp
public class ExceptionHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetPost_NotFound_ReturnsFormattedError()
    {
        // Test actual HTTP response format
    }

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsValidationErrors()
    {
        // Test validation error format
    }
}
```

**Step 6.3: Manual Testing Checklist**

- [ ] GET /api/Post/99999 returns 404 with correct format
- [ ] POST /api/Post with invalid data returns 422 with validation errors
- [ ] GET /api/Comment/99999 returns 404 with correct format
- [ ] POST /api/User/Register with existing email returns 400
- [ ] POST /api/User/Login with wrong credentials returns 400
- [ ] All error responses include TraceId
- [ ] Development environment shows stack traces
- [ ] Production environment hides sensitive details

### Phase 7: Documentation Updates (30 minutes)

**Step 7.1: Update API Documentation**

- Document error response formats in Swagger
- Add response examples for each status code

**Step 7.2: Update README**

- Document exception handling approach
- Add examples of error responses

### Implementation Checklist

#### Prerequisites

- [ ] Review current exception usage across codebase
- [ ] Ensure all tests pass before starting
- [ ] Create feature branch: `feature/global-exception-handling`

#### Phase 1: Foundation

- [ ] Create `PostHubException` base class
- [ ] Update `NotFoundException` to inherit from base
- [ ] Create `UnauthorizedException`
- [ ] Create `BadRequestException`
- [ ] Create `ValidationException`
- [ ] Create error response DTOs

#### Phase 2: Middleware

- [ ] Implement `GlobalExceptionHandler`
- [ ] Register in `Program.cs`
- [ ] Test middleware in isolation

#### Phase 3: Controller Cleanup

- [ ] Remove try-catch from `PostController`
- [ ] Remove try-catch from `CommentController`
- [ ] Remove try-catch from `UserController`
- [ ] Update service exception messages

#### Phase 4: Validation

- [ ] Create `ModelValidationFilter`
- [ ] Register validation filter
- [ ] Remove manual ModelState checks

#### Phase 5: Testing

- [ ] Write unit tests for exception handler
- [ ] Write integration tests for error responses
- [ ] Manual testing against checklist
- [ ] Performance testing (verify no degradation)

#### Phase 6: Documentation

- [ ] Update API documentation
- [ ] Update README
- [ ] Add code comments
- [ ] Update CHANGELOG

### Rollback Plan

If issues arise during implementation:

1. **Revert Program.cs changes** - Remove exception handler registration
2. **Keep new exception types** - They don't break existing code
3. **Restore try-catch blocks** - Git can restore previous version
4. **Test after rollback** - Ensure application works as before

### Estimated Effort

| Phase                  | Time           | Complexity |
| ---------------------- | -------------- | ---------- |
| Create Exception Types | 1-2 hours      | Low        |
| Create Response Models | 30 min         | Low        |
| Implement Handler      | 2-3 hours      | Medium     |
| Update Controllers     | 1-2 hours      | Low        |
| Add Validation Filter  | 1 hour         | Medium     |
| Testing                | 2-3 hours      | Medium     |
| Documentation          | 30 min         | Low        |
| **Total**              | **8-12 hours** | **Medium** |

### Success Criteria

✅ **Functionality**

- All existing API endpoints work without regression
- Error responses follow consistent format
- Appropriate HTTP status codes returned

✅ **Code Quality**

- Zero try-catch blocks in controllers
- All exceptions properly typed
- Logging implemented for all errors

✅ **Testing**

- 100% test coverage for exception handler
- All integration tests pass
- Manual testing checklist complete

✅ **Documentation**

- API documentation updated
- Code comments added
- CHANGELOG updated

### Benefits After Implementation

1. **Maintainability**: Single place to modify error handling
2. **Consistency**: All errors follow same format
3. **Observability**: Centralized logging with TraceId correlation
4. **Scalability**: Easy to add new exception types
5. **Testing**: Simpler controller tests (no exception paths)
6. **API Contract**: Predictable error responses for clients

### Potential Risks & Mitigations

| Risk                      | Impact | Mitigation                           |
| ------------------------- | ------ | ------------------------------------ |
| Breaking existing clients | High   | Maintain error message compatibility |
| Performance degradation   | Medium | Include middleware benchmarks        |
| Missing edge cases        | Medium | Comprehensive integration tests      |
| Incomplete migration      | Low    | Phases can be deployed incrementally |

### Next Steps After Completion

1. Consider implementing **ProblemDetails** RFC 7807 format
2. Add **correlation IDs** for distributed tracing
3. Integrate with **Application Insights** or monitoring tool
4. Consider **rate limiting** exceptions to prevent log flooding
5. Add **circuit breaker** pattern for downstream service failures
