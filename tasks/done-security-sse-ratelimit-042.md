# Task: Add per-IP rate limiting to SSE endpoint

**Module:** Messaging
**Priority:** MEDIUM (security)
**Source:** docs/audits/qa-night-security-20260401.md — 1.2

## Problem
SSE endpoint calls .DisableRateLimiting(). An attacker could open many SSE connections per IP before the broadcaster's per-clinic limit kicks in.

## Fix
Add a dedicated SSE rate limiter (e.g., 5 connections per IP) instead of fully disabling it.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] SSE endpoint has per-IP connection limit
