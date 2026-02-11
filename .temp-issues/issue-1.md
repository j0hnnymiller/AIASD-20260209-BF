## Severity: CRITICAL

### Description

JWT secret key is hardcoded in `appsettings.json` and committed to source control. This is a critical security vulnerability.

### Current Code

```json
"JWT": {
  "Secret": "JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr"
}
```

### Impact

- Anyone with repository access can forge authentication tokens
- Compromised security for all users
- Potential unauthorized access to the API

### Recommended Fix

1. Remove secret from `appsettings.json`
2. Use User Secrets for development:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "JWT:Secret" "your-secret-here"
   ```
3. Use environment variables or Azure Key Vault for production
4. Rotate the current secret immediately

### Files Affected

- `appsettings.json`
- `appsettings.Development.json`
- `Program.cs` (configuration reading)

### Priority

🔴 Critical - Fix Immediately
