## Severity: LOW

### Description

JWT token expiration time is hardcoded as magic number (3 hours).

### Current Code

```csharp
// UserService.cs line 74
expires: DateTime.Now.AddHours(3)
```

### Impact

- Difficult to change token expiration policy
- No environment-specific configuration
- Hard to manage for different scenarios (remember me, etc.)

### Recommended Fix

Move to configuration:

1. Add to appsettings.json:

```json
"JWT": {
  "TokenExpirationHours": 3,
  "RefreshTokenExpirationDays": 7
}
```

2. Update UserService:

```csharp
private JwtSecurityToken GetToken(IEnumerable<Claim> claims)
{
    var expirationHours = configuration.GetValue<int>("JWT:TokenExpirationHours");
    var authSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

    return new JwtSecurityToken(
        issuer: configuration["JWT:Issuer"],
        audience: configuration["JWT:Audience"],
        expires: DateTime.UtcNow.AddHours(expirationHours),
        claims: claims,
        signingCredentials: new SigningCredentials(
            authSigningKey, SecurityAlgorithms.HmacSha256)
    );
}
```

### Files Affected

- `Services/Implementations/UserService.cs`
- `appsettings.json`
- `appsettings.Development.json`

### Priority

🟢 Low - Configuration Issue
