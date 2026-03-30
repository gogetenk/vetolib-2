# todo-api-documentation-019.md -- Add API documentation (Scalar UI + endpoint summaries)

**Module** : Api
**Priority** : Moyenne
**Dependencies** : aucune

## Context
See `docs/studies/api-documentation-study-20260330.md`. 120 endpoints, almost zero documentation.

## Scope (quick wins first)
1. Add Scalar UI (`Scalar.AspNetCore` NuGet) — 30min
2. Declare JWT Bearer security scheme in OpenAPI — 1h
3. Add `.WithSummary()` + `.WithDescription()` to all endpoints — 4-6h
4. Add `.Produces()` for error responses (400, 401, 404, 409) — 2h

## Completion criteria
- [ ] Scalar UI accessible at /scalar in dev
- [ ] JWT Bearer auth documented in spec
- [ ] All endpoints have summaries
