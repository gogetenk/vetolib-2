# Task: Add missing integration tests for Messaging endpoints

**Module:** Messaging
**Priority:** HIGH (rule 3g violation)
**Source:** docs/audits/qa-night-b456-other-endpoints-20260401.md

## Problem
15 Messaging endpoints and 3 WhatsApp endpoints have 0 integration tests:
- Transfer conversation, recategorize, mark spam, get by ID
- Template CRUD (4 endpoints)
- Working hours settings, classification stats, accuracy
- File upload
- All 3 WhatsApp endpoints (no test file exists)

## Fix
1. Add integration tests in tests/Vetolib.Tests.Integration/Messaging/
2. Create WhatsAppEndpointsTests.cs
3. Each endpoint needs at least: authenticated success, unauthenticated 401

## Skills
- `ardalis-result`, `reqnroll-bindings`

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] At least 1 TI per untested Messaging endpoint
- [ ] WhatsApp test file created
