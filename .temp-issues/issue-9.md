## Severity: MEDIUM

### Description

UserService methods `Register` and `Login` are not async, blocking threads unnecessarily.

### Current Code

```csharp
// UserController.cs
public IActionResult Register([FromBody] RegisterUserDto dto)
{
    var token = userService.Register(dto); // Synchronous call
    return Ok(token);
}
```

### Impact

- Thread pool exhaustion under load
- Poor scalability
- Below ASP.NET Core best practices

### Recommended Fix

Make methods async:

```csharp
// IUserService.cs
Task<string> RegisterAsync(RegisterUserDto dto);
Task<string> LoginAsync(LoginUserDto dto);

// UserService.cs
public async Task<string> RegisterAsync(RegisterUserDto dto)
{
    // ... existing logic
    return await LoginAsync(new LoginUserDto { ... });
}

// UserController.cs
public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
{
    var token = await userService.RegisterAsync(dto);
    return Ok(token);
}
```

### Files Affected

- `Services/Interfaces/IUserService.cs`
- `Services/Implementations/UserService.cs`
- `Controllers/UserController.cs`

### Priority

🟡 Medium - Performance Issue
