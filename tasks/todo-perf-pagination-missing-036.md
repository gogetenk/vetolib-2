# Task: Add pagination to unbounded list endpoints (P-25, P-26, P-27)

**Module:** Breeding, Agenda, Messaging
**Priority:** HIGH
**Source:** docs/audits/qa-night-performance-20260401.md — P-25, P-26, P-27

## Problem
- GET /api/v1/patients/{id}/heat-cycles — no pagination
- GET /api/v1/waitlist — no pagination
- GET /api/v1/messaging/portal/{ownerId}/conversations — no pagination

## Fix
Add page/pageSize query parameters to each endpoint. Add Skip/Take to handlers.
Follow existing pagination pattern from appointments/patients endpoints.

## Skills
- `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Heat cycles endpoint accepts page/pageSize, defaults to page=1, pageSize=20
- [ ] Waitlist endpoint accepts page/pageSize
- [ ] Owner conversations endpoint accepts page/pageSize
- [ ] Handlers use .Skip() + .Take() correctly
