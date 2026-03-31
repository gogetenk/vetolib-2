# Task: Cap pageSize on all paginated endpoints

**Module:** Multiple
**Priority:** MEDIUM
**Source:** docs/audits/qa-night-security-20260401.md — 3.3

## Problem
Several endpoints accept pageSize without upper bound. A malicious user could send pageSize=999999.

## Fix
In each paginated endpoint, clamp pageSize to max 200:
- GET /api/v1/users
- GET /api/v1/invoices
- GET /api/v1/messaging/conversations
- GET /api/v1/appointments
- Any other paginated endpoint

Follow the pattern from AuditEndpoints.cs: `if (pageSize is < 1 or > 200) pageSize = 50;`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] All paginated endpoints cap pageSize at 200
