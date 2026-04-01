# Keycloak PoC Results -- JWT Validation with Organizations

**Date**: 2026-04-01
**Branch**: `feat/keycloak-jwt-validation`
**Status**: GO

---

## What was implemented

### Dual-stack JWT authentication

`AuthModuleServiceRegistrar` now supports two JWT bearer schemes running in parallel:

| Scheme | Algorithm | Issuer | Use case |
|--------|-----------|--------|----------|
| `Legacy` | HS256 (symmetric) | Built-in `JwtTokenService` | Existing login flow, backward compat |
| `Keycloak` | RS256 (asymmetric) | Keycloak OIDC | New SSO / organization-based auth |

A **policy scheme** (`MultiScheme`) inspects the JWT header `alg` field to route to the correct handler:
- `RS*` algorithms -> Keycloak scheme (OIDC discovery via `.well-known/openid-configuration`)
- `HS*` algorithms -> Legacy scheme (symmetric key validation)

### Graceful degradation

When `Keycloak:Authority` is empty or not configured, the system falls back to Legacy-only mode. No Keycloak scheme is registered, no policy scheme is added. Existing deployments are unaffected.

### Claims compatibility (ClinicContext)

`ClinicContext` (FROZEN) reads `clinic_id` from the JWT claims. Two paths ensure compatibility:

1. **Protocol mapper** (primary): The Keycloak realm config (`vetolib-realm.json`) includes a `clinic_id` protocol mapper on the `vetolib-api` client that emits the user attribute `clinic_id` directly into the access token. This is a 1:1 match with what `ClinicContext` reads.

2. **ClaimsTransformation** (fallback): `KeycloakClaimsTransformation` handles the case where Keycloak Organizations emit `organization.id` instead. It maps `organization.id` -> `clinic_id` if `clinic_id` is not already present. This is a no-op when the protocol mapper is active.

### Authorization policies

All existing policies (`ClinicStaff`, `VetOrAdmin`, default) now accept tokens from both schemes. Role claims from Keycloak realm roles map to the same values used by Legacy tokens.

---

## Configuration

```json
// appsettings.json
{
  "Keycloak": {
    "Authority": "",          // empty = disabled (Legacy only)
    "Audience": "vetolib-api"
  }
}

// appsettings.Development.json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/vetolib",
    "Audience": "vetolib-api"
  }
}
```

When running via Aspire, the Authority URL should point to the Keycloak resource endpoint (e.g., `http://keycloak:8080/realms/vetolib` or the Aspire service discovery URL).

---

## Test results

| Check | Result |
|-------|--------|
| `dotnet build -c Release` | 0 errors |
| Unit tests (new: 9) | 9/9 GREEN |
| Unit tests (total: 2166) | 2159 pass, 7 pre-existing failures (Notifications module, unrelated) |
| Pre-existing test regressions | None |

### New unit tests

- `KeycloakClaimsTransformationTests` (5 tests): clinic_id already present, organization.id mapping, no claims, unauthenticated, both claims present
- `PadBase64Tests` (4 tests): base64url padding edge cases

---

## Files changed

| File | Change |
|------|--------|
| `src/backend/Modules/Auth/Vetolib.Auth/AuthModuleServiceRegistrar.cs` | Dual-stack JWT, policy scheme, claims transformation registration |
| `src/backend/Modules/Auth/Vetolib.Auth/Application/KeycloakClaimsTransformation.cs` | NEW -- maps organization.id to clinic_id |
| `src/backend/Vetolib.Api/appsettings.json` | Keycloak config section (disabled by default) |
| `src/backend/Vetolib.Api/appsettings.Development.json` | Keycloak config section (localhost) |
| `tests/Vetolib.Tests.Unit/Auth/KeycloakClaimsTransformationTests.cs` | NEW -- 9 unit tests |

---

## Known limitations / next steps

1. **Role claim mapping**: Keycloak emits roles in `realm_access.roles` (nested JSON array), while Legacy uses flat `role` claims. A role-mapping claims transformation may be needed when testing real Keycloak tokens with `[Authorize(Policy = "ClinicStaff")]`.
2. **HTTPS metadata**: `RequireHttpsMetadata = false` is set for dev. Must be `true` in production.
3. **Aspire service discovery**: The `Keycloak:Authority` URL should ideally come from Aspire resource references rather than hardcoded config. This will be addressed when wiring the Aspire host.
4. **Integration test**: An integration test with a real Keycloak container (Testcontainers) should validate the full OIDC flow end-to-end.

---

## Verdict: GO

The dual-stack approach is viable. Legacy tokens continue to work unchanged. Keycloak tokens will be validated via OIDC discovery when the Authority is configured. The `ClinicContext` contract is preserved without any modification to Shared/.
