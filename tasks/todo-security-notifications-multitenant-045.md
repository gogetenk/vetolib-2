# Task: Fix NotificationsDbContext missing multi-tenancy

**Module:** Notifications
**Priority:** CRITICAL (security — cross-tenant data exposure)
**Source:** QA migrations consistency audit (agent a075c35f)

## Problem
NotificationsDbContext inherits from plain `DbContext` instead of `MultiTenantDbContext`.
Both `ReminderLog` and `ReminderConfig` implement `IMultiTenant` but NO automatic tenant filter is applied.
Queries return data from ALL clinics — potential cross-tenant data exposure.

File: `src/backend/Modules/Notifications/Vetolib.Notifications/Infrastructure/NotificationsDbContext.cs`

## Fix
1. Change `NotificationsDbContext` to inherit from `MultiTenantDbContext`
2. Update constructor to accept `IClinicContext` and `IPublisher` (same pattern as other modules)
3. Verify OnModelCreating calls `base.OnModelCreating()` first
4. Generate migration if needed
5. MODIF_SHARED: NOT needed — MultiTenantDbContext is in Shared but we're only inheriting from it

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] NotificationsDbContext inherits MultiTenantDbContext
- [ ] Tenant filter applied automatically to ReminderLog and ReminderConfig
