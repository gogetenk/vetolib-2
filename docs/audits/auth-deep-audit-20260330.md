# Auth Module Deep Audit - 2026-03-30

**Scope**: Every `.cs` file in `Vetolib.Auth/` and `Vetolib.Auth.Contracts/`
**Auditor**: Claude Opus 4.6
**Status**: Report only - no fixes applied

---

## CRITICAL - Security Issues

### SEC-01: Temporary password in domain event violates least privilege
**Files**: `Domain/User.cs:86`, `Domain/Events/UserInvitedDomainEvent.cs`, `Events/UserInvitedDomainEventHandler.cs`, `Contracts/UserInvitedIntegrationEvent.cs`

The plaintext temporary password flows through:
1. `User.Invite()` adds `UserInvitedDomainEvent` containing the raw password
2. `UserInvitedDomainEventHandler` publishes it as `UserInvitedIntegrationEvent` to RabbitMQ
3. The integration event carries `TemporaryPassword` in plaintext across the message bus

The temporary password is persisted in the MassTransit outbox tables (`auth.outbox_message`) as serialized JSON. This means plaintext passwords sit in the database in the outbox, surviving well beyond the invitation flow. If the outbox is ever inspected, backed up, or leaked, passwords are exposed.

**Severity**: Critical

### SEC-02: No `MustChangePassword` flag enforcement
**File**: `Domain/User.cs`, `Commands/Login/LoginHandler.cs`

The migration `20260309160059_AddMustChangePassword.cs` adds a `MustChangePassword` column, but the `User` entity has **no corresponding property** and the `LoginHandler` does **not check** whether the user must change their password on first login. Invited users get a temporary password but are never forced to change it.

**Severity**: Critical

### SEC-03: Race condition on refresh token rotation
**File**: `Commands/RefreshToken/RefreshTokenHandler.cs`

Token rotation is not atomic. Between the `FirstOrDefaultAsync` (line 24) and `SaveChangesAsync` (line 50), a concurrent request with the same refresh token could:
1. Both read the same valid token
2. Both revoke it and issue new tokens
3. Result: two valid refresh token chains from one token

No optimistic concurrency token or unique constraint prevents this. In a multi-instance deployment, this is a token replay vulnerability.

**Severity**: High

### SEC-04: SubscriptionCheckFilter reads wrong claim name
**File**: `Api/SubscriptionCheckFilter.cs:44`

The filter reads `clinicId` claim (camelCase):
```csharp
var clinicIdClaim = context.HttpContext.User.FindFirst("clinicId")?.Value;
```

But `JwtTokenService.cs:39` writes the claim as `clinic_id` (snake_case):
```csharp
new Claim("clinic_id", clinicId.ToString()),
```

The subscription check filter will **never find the claim** and always return 403 Forbidden, effectively making all subscription-gated endpoints inaccessible.

**Severity**: High

### SEC-05: `IsCurrentlyLocked()` has side-effect-free lock expiry
**File**: `Domain/User.cs:128-136`

`IsCurrentlyLocked()` returns `false` when the lock has expired but does NOT reset `IsLocked`/`FailedLoginAttempts`/`LockedUntil`. The `LoginHandler` (line 46-49) does call `Unlock()` in this case, but only on a successful path. If `IsCurrentlyLocked()` is called from other contexts, the stale `IsLocked = true` persists even though the lock has expired.

**Severity**: Medium

### SEC-06: Deactivated user tokens remain valid
**Files**: `Commands/DeactivateUser/DeactivateUserHandler.cs`

When a user is deactivated, their existing refresh tokens are **not revoked**. The deactivated user can continue using their current access token until it expires and can refresh it to get new tokens. The `RefreshTokenHandler` does not check `user.IsActive` before issuing new tokens.

Wait - it does check `user.IsActive` indirectly: `RefreshTokenHandler` line 37 loads user but does NOT check `IsActive`. A deactivated user can successfully refresh tokens.

**Severity**: High

---

## HIGH - Correctness Issues

### COR-01: Password validation rules are inconsistent across endpoints
**Files**: `Domain/User.cs:170-183`, `Commands/CreateUser/CreateUserValidator.cs`, `Commands/RegisterClinic/RegisterClinicValidator.cs`, `Commands/ChangePassword/ChangePasswordValidator.cs`

Four different password policies:
| Source | Min length | Uppercase | Digit | Special char |
|---|---|---|---|---|
| `User.ValidatePassword` (Domain) | 8 | Yes | Yes | No |
| `CreateUserValidator` | 8 | Yes | Yes | No |
| `RegisterClinicValidator` | 10 | No | No | Yes |
| `ChangePasswordValidator` | 10 | Yes | Yes | Yes |

The domain model (`User.Create`) allows 8-char passwords with no special characters. `RegisterClinic` requires 10 chars + special char but no uppercase/digit. `ChangePassword` is the strictest (10 + upper + digit + special). These inconsistencies mean:
- A user registered via `RegisterClinic` might have a password that `ChangePassword` would reject
- `CreateUser` allows weaker passwords than `RegisterClinic`
- The `Invite` flow calls `User.Invite` which does NOT validate the temporary password at all (skipped entirely)

**Severity**: High

### COR-02: `GetUsersQuery` and `ListUsersQuery` are exact duplicates
**Files**: `Queries/GetUsers/` and `Queries/ListUsers/`

Both queries have identical implementations: same handler logic, same DTO return type, same pagination. `GetUsersHandler` and `ListUsersHandler` are copy-paste duplicates. The `AuthEndpoints` uses neither - `UserEndpoints.GetUsers` dispatches `ListUsersQuery`. `GetUsersQuery`/`GetUsersHandler` appear to be dead code.

**Severity**: Medium (dead code)

### COR-03: `CreateUserHandler` email uniqueness check is tenant-scoped but should be global
**File**: `Commands/CreateUser/CreateUserHandler.cs:22-23`

```csharp
var existingUser = await _context.Users
    .FirstOrDefaultAsync(u => u.Email == cmd.Email.ToLowerInvariant(), ct);
```

This uses the tenant-filtered DbContext (no `IgnoreQueryFilters()`), so it only checks within the current clinic. However, the `LoginHandler` finds users by email cross-tenant. If the same email exists in two clinics, login will always find the first one created (ordered by DB), making the second account unreachable.

Compare with `RegisterClinicHandler` (line 29-31) and `InviteUserHandler` (line 26-27) which also only check within tenant scope. The `RegisterClinicHandler` correctly uses `IgnoreQueryFilters()` but `InviteUserHandler` does not.

**Severity**: High

### COR-04: `ClinicVetReader.GetVeterinariansForClinic` bypasses tenant filter but `SubscriptionChecker.CheckLimitAsync` has mixed approaches
**File**: `Services/SubscriptionChecker.cs:38-39`

```csharp
await _dbContext.Users.CountAsync(u => u.ClinicId == clinicId && u.Role == UserRole.Vet && u.IsActive, ct),
```

This does NOT use `IgnoreQueryFilters()` for counting vets in `CheckLimitAsync`, meaning the multi-tenant filter is still applied. If `IClinicContext.ClinicId` differs from the `clinicId` parameter, the count will be wrong. Meanwhile, `ClinicVetReader` correctly uses `IgnoreQueryFilters()`.

Similarly in `GetCurrentUsageAsync` (line 88-89), the vet count uses the filtered context.

**Severity**: High

### COR-05: `RegisterClinicHandler` has two `SaveChangesAsync` calls without transaction
**File**: `Commands/RegisterClinic/RegisterClinicHandler.cs:52-64`

First `SaveChangesAsync` (line 53) persists clinic + user. Second `SaveChangesAsync` (line 64) persists the refresh token. If the second fails, the clinic and user exist but the user has no token - the response would error, but the registration is half-complete. The user would need to login manually. Not catastrophic but inconsistent.

**Severity**: Medium

### COR-06: `SwitchClinicHandler` does not verify user has a valid role in target clinic
**File**: `Commands/SwitchClinic/SwitchClinicHandler.cs`

The handler allows switching to any clinic in the user's group, but the JWT token carries the **original user's role** (from their home clinic). A receptionist in Clinic A who switches to Clinic B gets the same Receptionist role even if they have no account in Clinic B. The handler explicitly notes this is a "virtual" view (line 42-44). This means authorization policies apply based on the home clinic role, which may be incorrect for the target clinic.

**Severity**: Medium

---

## MEDIUM - Validation & Design Issues

### VAL-01: Missing validators for several commands
**Files**:

The following commands have **no FluentValidation validator**:
- `AddClinicToGroupCommand` - no validator file exists
- `RemoveClinicFromGroupCommand` - no validator file exists
- `SwitchClinicCommand` - no validator file exists

While the handlers do null/empty checks, the validation pipeline won't catch bad input before the handler executes.

**Severity**: Medium

### VAL-02: `InviteUserValidator` does not validate `Role`
**File**: `Commands/InviteUser/InviteUserValidator.cs`

The validator checks Email, FullName, ClinicId but not `Role`. An invalid enum value could pass validation. The `CreateUserValidator` correctly includes `RuleFor(x => x.Role).IsInEnum()`.

**Severity**: Low

### VAL-03: `RegisterClinicCommand` fields `Phone` and `Country` are unused
**File**: `Commands/RegisterClinic/RegisterClinicCommand.cs`, `RegisterClinicHandler.cs`

The command accepts `Phone` and `Country` and the validator requires them non-empty, but the handler never uses them. They are not stored on the `Clinic` entity nor passed anywhere.

**Severity**: Low (dead fields)

### VAL-04: `OnboardingSteps.GetStepsForRole` default case returns Admin steps
**File**: `Domain/OnboardingSteps.cs:37`

```csharp
_ => [InviteTeamMember, AddFirstPatient, ...]
```

An unknown role string silently gets Admin onboarding steps. This should either return an empty list or fail explicitly.

**Severity**: Low

---

## MEDIUM - Multi-Tenancy Concerns

### MT-01: `RefreshToken` entity is not `IMultiTenant` but queried without `IgnoreQueryFilters()`
**Files**: `Domain/RefreshToken.cs`, `Commands/Logout/LogoutHandler.cs:19`, `Commands/ChangePassword/ChangePasswordHandler.cs:34`

`RefreshToken` does not implement `IMultiTenant`. The `MultiTenantDbContext` base class only applies filters to entities implementing `IMultiTenant`, so no filter is applied. This is correct behavior but undocumented - it relies on implicit knowledge that non-`IMultiTenant` entities skip the filter.

The `RefreshTokenHandler` comment on line 23 acknowledges this: "no tenant filter on RefreshToken since it's not IMultiTenant". Other handlers querying `RefreshTokens` don't document this assumption.

**Severity**: Informational

### MT-02: `ClinicGroup` and `ClinicGroupMember` are cross-tenant but no authorization check on group ownership
**Files**: `Commands/AddClinicToGroup/AddClinicToGroupHandler.cs`, `Commands/RemoveClinicFromGroup/RemoveClinicFromGroupHandler.cs`, `Queries/ListGroupClinics/ListGroupClinicsHandler.cs`

None of these handlers verify that the requesting user is the group owner (`OwnerUserId`). The endpoint requires Admin role, but any Admin from any clinic can add/remove clinics to/from any group they know the ID of. The `ClinicGroupEndpoints` checks `role == "Admin"` but does not verify group ownership.

**Severity**: High (authorization bypass)

---

## LOW - Code Quality

### CQ-01: Hardcoded fallback clinic name
**File**: `Commands/InviteUser/InviteUserHandler.cs:36`

```csharp
var clinicName = _configuration["ClinicName"] ?? "Desert Paws Veterinary Clinic";
```

This should read the clinic name from the `Clinic` entity in the database, not from configuration with a hardcoded UAE-specific fallback.

**Severity**: Low

### CQ-02: French error messages mixed with English
**Files**: `Commands/RefreshToken/RefreshTokenHandler.cs:28,38`, `Commands/CreateUser/CreateUserHandler.cs:27`

```csharp
"INVALID_REFRESH_TOKEN:Le refresh token est invalide ou expire"
"INVALID_REFRESH_TOKEN:Utilisateur non trouve"
"EMAIL_EXISTS:Cet email est deja utilise"
```

Inconsistent language. Other error messages are in English. For a UAE-market product, all messages should be in English (or use i18n).

**Severity**: Low

### CQ-03: `CheckLimitAttribute` is `public` but should be `internal`
**File**: `Api/SubscriptionCheckFilter.cs:12`

```csharp
public class CheckLimitAttribute : Attribute
```

Per project rules, the only public class in the runtime assembly should be `AuthModuleServiceRegistrar`. `CheckLimitAttribute` is public, violating the 2-assembly isolation rule.

**Severity**: Low (architecture rule violation)

### CQ-04: No index on `RefreshToken.Token` for equality lookups
**File**: `Infrastructure/RefreshTokenConfiguration.cs:29`

There IS an index on `Token` (line 29), but it's a non-unique B-tree index. The `RefreshTokenHandler` queries by exact token match. This is fine for performance but the token column stores Base64 strings of 88 chars - might benefit from a hash index in PostgreSQL.

**Severity**: Informational

### CQ-05: `User.FullName` is empty string on `User.Create` but required on `User.Invite`
**File**: `Domain/User.cs:46,78`

`User.Create` sets `FullName = string.Empty` and there is no way to set it later (no `SetFullName` method). Users created via `CreateUser` will always have an empty FullName, which shows in `UserListItemDto`.

**Severity**: Low (missing feature)

---

## TODO/HACK Comments Found

### TODO-01: WhatsApp message counting stub
**File**: `Services/SubscriptionChecker.cs:146`
```csharp
// TODO: Wire to Messaging module once GetWhatsAppMessageCountQuery is added
```

### TODO-02: Storage tracking stub
**File**: `Services/SubscriptionChecker.cs:153`
```csharp
// TODO: Wire to storage tracking once GetStorageUsedQuery is available
```

Both stubs return 0, meaning WhatsApp and storage limits are never enforced.

---

## Race Conditions

### RACE-01: Concurrent user creation with same email
**File**: `Commands/CreateUser/CreateUserHandler.cs`, `Commands/RegisterClinic/RegisterClinicHandler.cs`, `Commands/InviteUser/InviteUserHandler.cs`

All three check "does email exist?" then insert. Between the check and insert, another request could insert the same email. The unique index `(ClinicId, Email)` on the `users` table will throw a `DbUpdateException` on the second insert, but this exception is not caught - it will bubble up as a 500 error instead of a clean duplicate-email message.

For `RegisterClinicHandler`, the unique index is scoped to `(ClinicId, Email)` but different clinics could have the same email (different ClinicId). The `IgnoreQueryFilters` check prevents this at application level, but the DB schema does not enforce global email uniqueness. Under concurrent registration, two clinics could be created with the same admin email.

**Severity**: Medium

### RACE-02: Concurrent login attempts and lockout counter
**File**: `Commands/Login/LoginHandler.cs`

`RecordFailedLogin` increments `FailedLoginAttempts` in memory, then `SaveChangesAsync` persists. Two concurrent failed login attempts could both read `FailedLoginAttempts = 4`, both increment to 5, but only one write wins. The lockout threshold could be bypassed by 1 attempt (5 attempts allowed but 6 needed to trigger).

**Severity**: Low (minor off-by-one under concurrent load)

---

## Summary

| Category | Critical | High | Medium | Low | Info |
|---|---|---|---|---|---|
| Security | 2 | 3 | 1 | 0 | 0 |
| Correctness | 0 | 3 | 2 | 0 | 0 |
| Validation | 0 | 0 | 1 | 3 | 0 |
| Multi-Tenancy | 0 | 1 | 0 | 0 | 1 |
| Code Quality | 0 | 0 | 0 | 3 | 2 |
| Race Conditions | 0 | 0 | 1 | 1 | 0 |
| **Total** | **2** | **7** | **5** | **7** | **3** |

### Top 5 items to fix first
1. **SEC-01**: Remove plaintext password from domain/integration events - use a time-limited invite token instead
2. **SEC-04**: Fix claim name mismatch `clinicId` vs `clinic_id` in SubscriptionCheckFilter
3. **SEC-06**: Revoke refresh tokens on user deactivation, check `IsActive` in RefreshTokenHandler
4. **MT-02**: Add group ownership verification in AddClinicToGroup/RemoveClinicFromGroup handlers
5. **COR-03**: Use `IgnoreQueryFilters()` for email uniqueness in CreateUser and InviteUser, or add a global unique index on email
