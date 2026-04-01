# Task: Fix 20 pre-existing TI alignment failures

**Module:** Multiple (Breeding, AI, Messaging, Stock)
**Priority:** HIGH (CI RED on TI)
**Source:** CI run on develop — 20 TI failures, 186/206 passing

## Problem
20 integration tests fail with status code mismatches (e.g., expect 200 get 422/400).
Root cause: endpoint signatures changed (pagination added, request body changed) but TI tests not updated.

Most failures are in:
- Breeding: 17 failures (pagination changes on heat-cycles, new request formats)
- AI: 1 failure (PredictNoShowBatch)
- Messaging: 1 failure (SendMessage portal)
- Stock: 1 failure (RecordMovement)

## Fix
1. Run integration tests locally with Docker (Testcontainers)
2. For each failure, read the endpoint and update the test to match the new signature
3. Update request payloads, expected status codes, deserialization types

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` TI GREEN (206/206 passing)
