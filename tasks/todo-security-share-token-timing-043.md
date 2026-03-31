# Task: Use constant-time comparison for share link tokens

**Module:** MedicalRecords
**Priority:** MEDIUM (security)
**Source:** docs/audits/qa-night-security-20260401.md — 4.4

## Problem
GetSharedRecordHandler uses 7 IgnoreQueryFilters() calls for a public anonymous endpoint. Token comparison may be vulnerable to timing attacks.

## Fix
1. Ensure share link token comparison uses CryptographicOperations.FixedTimeEquals
2. Add logging for failed token lookups (brute-force detection)

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Token comparison uses constant-time method
- [ ] Failed lookups are logged
