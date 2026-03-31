# Task: Fix missing rate limiting on GET /api/v1/patients/{id}/litters

**Module:** Breeding
**Priority:** HIGH
**Source:** QA endpoint audit (agent a4e124ba) — B456 report

## Problem
In LitterEndpoints.cs, `GET /api/v1/patients/{id}/litters` is registered outside the rate-limited group.

## Fix
Add .RequireRateLimiting("api") to the endpoint.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Endpoint has rate limiting
