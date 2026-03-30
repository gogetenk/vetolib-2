# todo-security-critical-016.md -- Fix CRITICAL + HIGH security issues

**Module** : Auth, Api
**Priority** : Critique
**Dependencies** : aucune

## Scope

### S-01 CRITICAL: Replace System.Random with RandomNumberGenerator
- File: InviteUserHandler.cs
- Replace `new Random().Next()` with `RandomNumberGenerator.GetBytes()` for temp password

### S-02 HIGH: Add CORS middleware
- Add CORS policy in Program.cs (allow configured origins)

### S-03 HIGH: Add global exception handler
- Add middleware that catches unhandled exceptions and returns 500 without stack trace

### S-04 HIGH: Restrict OpenAPI to development
- Wrap `app.MapOpenApi()` in `if (app.Environment.IsDevelopment())`

### S-05 HIGH: Add HTTPS redirection
- Add `app.UseHttpsRedirection()` before auth middleware

### S-06 HIGH: Remove temp password from API response
- InviteUserHandler should NOT return the temp password in the response

## Completion criteria
- [ ] All 6 fixes applied
- [ ] `dotnet build` + `dotnet test` GREEN
