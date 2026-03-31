# Task: Add OutputCache to frequently-read endpoints

**Module:** Multiple
**Priority:** MEDIUM (performance)
**Source:** docs/audits/qa-night-performance-20260401.md — P-30

## Problem
These read-heavy, rarely-changing endpoints have no OutputCache:
- GET /api/v1/consultation-types
- GET /api/v1/messaging/settings/hours
- GET /api/v1/stock/alerts
- GET /api/v1/preferences/working-hours

## Fix
Add CacheOutput("Moderate2min") or similar to each endpoint.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Listed endpoints have CacheOutput configured
