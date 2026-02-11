## Severity: CRITICAL

### Description

HTTPS metadata validation is disabled in JWT configuration, allowing tokens to be transmitted over insecure connections.

### Current Code

```csharp
// Program.cs line 39
opts.RequireHttpsMetadata = false;
```

### Impact

- Tokens can be intercepted over HTTP connections
- Man-in-the-middle attacks possible
- Violates security best practices

### Recommended Fix

Remove this line or set to `true`:

```csharp
opts.RequireHttpsMetadata = true; // or remove completely (true is default)
```

### Files Affected

- `Program.cs`

### Priority

🔴 Critical - Security Vulnerability
