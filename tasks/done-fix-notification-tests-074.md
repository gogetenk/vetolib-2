# Task: Fix 6 pre-existing Notification unit test failures

**Module:** Notifications
**Priority:** HIGH (tests should be GREEN)
**Source:** Multiple agents reported this across sessions

## Problem
6 tests in AppointmentReminderConsumerTests fail when run as part of the full suite but pass individually. This is a test ordering/state isolation issue.

Likely cause: NotificationsDbContext was recently changed to inherit MultiTenantDbContext (PR #307), which requires IClinicContext and IPublisher in constructor. Some tests may not provide these correctly.

## Fix
1. Read tests/Vetolib.Tests.Unit/Notifications/AppointmentReminderConsumerTests.cs
2. Read tests/Vetolib.Tests.Unit/Notifications/ReminderSchedulerServiceTests.cs
3. Verify mock setup for IClinicContext provides a valid ClinicId
4. Fix the test isolation issue (likely shared static state or missing mock setup)

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] All 6 Notification tests pass in full suite run
- [ ] `dotnet test tests/Vetolib.Tests.Unit/ -c Release` → 0 failures
