# Task: Add AsNoTracking to all query handlers (P-06 to P-10)

**Module:** Breeding, Agenda, Messaging, AI
**Priority:** MEDIUM
**Source:** docs/audits/qa-night-performance-20260401.md — P-06 to P-10

## Problem
Multiple query handlers missing .AsNoTracking():
- PredictNextHeatHandler (Breeding)
- GetClassificationAccuracyHandler (Messaging)
- ListWaitlistEntriesHandler (Agenda)
- GetVisitFeedbackStatsHandler (Agenda)
- GenerateHealthAlertsHandler dedup query (AI)

## Fix
Add .AsNoTracking() before .ToListAsync() on all read-only queries.

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] All listed handlers use .AsNoTracking()
