# Task: Replace manual role checks with RequireAuthorization policies

**Module:** Auth, MedicalRecords
**Priority:** HIGH
**Source:** docs/audits/qa-night-security-20260401.md — 6.1

## Problem
UserEndpoints, ClinicGroupEndpoints, MedicalRecordEndpoints, MedicalRecordTemplateEndpoints, OwnerEndpoints manually check role claims instead of using ASP.NET Core's built-in RequireAuthorization.

## Fix
Replace inline role checks like:
```csharp
var role = user.FindFirst(ClaimTypes.Role)?.Value;
if (role != "Admin") return Results.Forbid();
```
With endpoint-level policy:
```csharp
.RequireAuthorization(policy => policy.RequireRole("Admin"))
```

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] No manual ClaimTypes.Role checks in endpoint delegates
- [ ] All Admin-only endpoints use .RequireAuthorization() with role policy
