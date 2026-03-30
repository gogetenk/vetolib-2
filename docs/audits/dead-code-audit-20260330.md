# Dead Code & Unused Dependencies Audit

**Date**: 2026-03-30
**Scope**: Full codebase scan (backend + frontend)
**Action**: Report only -- no fixes applied

---

## 1. Unused NuGet Packages

### HIGH confidence (package referenced but zero `using` or API usage in the project)

| Project | Package | Evidence |
|---|---|---|
| `src/backend/Vetolib.Api/Vetolib.Api.csproj` | `MassTransit` + `MassTransit.RabbitMQ` | The API host project has no consumers, publishers, or MassTransit configuration code. MassTransit is configured in individual modules. These references in the API host appear redundant. |

### MEDIUM confidence (package present, but only used in scaffolding/future wiring)

| Project | Package | Notes |
|---|---|---|
| `src/frontend/package.json` | `@fontsource/manrope` (line 20) | **Unused.** The app uses `next/font/google` Manrope import in `src/app/[locale]/layout.tsx:2`. The `@fontsource` package is never imported anywhere. |

---

## 2. Unused npm Packages

| Package | Type | Evidence |
|---|---|---|
| `@fontsource/manrope` | dependency | Zero imports in `src/`. Font loaded via `next/font/google` instead (see `src/app/[locale]/layout.tsx:2`). |

All other dependencies (`@base-ui/react`, `class-variance-authority`, `clsx`, `date-fns`, `recharts`, `sonner`, `posthog-js`, `react-hook-form`, `@hookform/resolvers`, `@tanstack/react-table`, `tailwind-merge`, `zod`, `next-themes`, `next-intl`, `lucide-react`) are confirmed in use.

---

## 3. Dead Code Files

### Backend -- Unused Notification Contract Events

These event records are defined but **never published or consumed** anywhere in the codebase:

| File | Type |
|---|---|
| `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Events/InvoiceSentEvent.cs` | Record defined, zero references outside its own file |
| `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Events/SendEmailRequest.cs` | Record defined, zero references outside its own file |
| `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Events/UserInvitedEvent.cs` | Record defined, zero references outside its own file |
| `src/backend/Modules/Notifications/Vetolib.Notifications.Contracts/Events/AppointmentReminderEvent.cs` | Record defined, zero references outside its own file |

**Note**: `ReminderConfigDto`, `ReminderLogDto`, `DeliveryStatus`, `NotificationChannel`, and `ReminderType` in the same Contracts project ARE used by the Notifications runtime module.

### Frontend -- Dead Utility Files

| File | Evidence |
|---|---|
| `src/frontend/src/lib/env.ts` | Exports `env.apiUrl` but it is **never imported** by any other file. Only self-references in a comment. |
| `src/frontend/src/lib/format-category.ts` | Exports `formatCategory()` function -- zero imports anywhere in `src/`. The function is never called. |
| `src/frontend/write_qa.py` | 1-line file (`# QA Report`) with no content and no references in any script or CI config. |

### Frontend -- Stale Test Artifacts (committed or tracked)

| File | Notes |
|---|---|
| `src/frontend/test-results.json` | Test output artifact -- should be in `.gitignore`, not tracked. |
| `src/frontend/test-results-screenshots/` | Test output directory -- should be in `.gitignore`, not tracked. |

---

## 4. Dead Feature Flags / TODO Markers / Disabled Features

### Backend TODOs (stub methods returning hardcoded values)

| File | Line | Content |
|---|---|---|
| `src/backend/Modules/Auth/Vetolib.Auth/Application/Services/SubscriptionChecker.cs` | 146 | `// TODO: Wire to Messaging module once GetWhatsAppMessageCountQuery is added to Vetolib.Messaging.Contracts` -- Method `CountWhatsAppMessagesThisMonthAsync` returns hardcoded `0`. |
| `src/backend/Modules/Auth/Vetolib.Auth/Application/Services/SubscriptionChecker.cs` | 153 | `// TODO: Wire to storage tracking once GetStorageUsedQuery is available` -- Method `GetStorageUsedGBAsync` returns hardcoded `0`. |

No `#if false`, `#if NEVER`, `// HACK`, `// FIXME`, or `// DISABLED` markers were found in the backend.

No TODO/HACK/FIXME markers were found in the frontend TypeScript/TSX files.

---

## 5. Stale MSW Handlers

All 19 MSW handler files in `src/frontend/src/mocks/handlers/` are registered in the index and correspond to active frontend features. No orphaned or stale handlers detected.

Handler files checked: `auth`, `billing`, `appointments`, `patients`, `users`, `dashboard`, `messaging`, `drugs`, `portal`, `onboarding`, `preferences`, `stock`, `ai`, `booking`, `clinic-group`, `reminders`, `health-alerts`, `weights`, `breeding`.

**No stale MSW handlers found.**

---

## 6. Unused CSS / Tailwind Custom Classes

### globals.css -- Unused Custom Animation Classes

| Class | Line | Evidence |
|---|---|---|
| `.animate-auth-check` | 184 | Defined in `globals.css` but **zero usage** in any `.tsx` or `.ts` file. |
| `.animate-typing-dot` | 248 | Defined in `globals.css` but **zero usage** in any `.tsx` or `.ts` file. |

### globals.css -- Unused CSS Variable

| Variable | Line | Evidence |
|---|---|---|
| `--font-geist-mono` (via `--font-mono`) | 10 | The CSS variable `--font-mono: var(--font-geist-mono)` references a font variable that is **never set** by any layout or config file. No Geist Mono font is loaded. The `font-mono` utility class IS used in 5 component files, but they all resolve to the fallback system monospace stack since `--font-geist-mono` is never defined. |

### globals.css -- All Other Custom Classes Are Used

The following custom classes were confirmed in use:
- `.animate-auth-card-in` (3 files), `.animate-auth-error-slide` (2 files), `.animate-auth-shake` (2 files)
- `.auth-link-underline` (2 files)
- `.animate-shimmer` (1 file), `.animate-pulse-badge` (2 files), `.animate-shake` (2 files)
- `.animate-slide-up-fade` (5 files), `.animate-stagger-fade-in` (1 file), `.animate-success-check` (1 file)

---

## Summary

| Category | Items Found | Severity |
|---|---|---|
| Unused NuGet packages | 1 (MassTransit in Api host) + 1 medium (fontsource) | Low |
| Unused npm packages | 1 (`@fontsource/manrope`) | Low |
| Dead backend contract files | 4 Notification events | Medium |
| Dead frontend utility files | 2 (`env.ts`, `format-category.ts`) | Low |
| Dead misc files | 1 (`write_qa.py`) | Low |
| Stale test artifacts | 2 (should be gitignored) | Low |
| TODO stubs (hardcoded returns) | 2 methods in SubscriptionChecker | Info |
| Unused CSS classes | 2 (`.animate-auth-check`, `.animate-typing-dot`) | Low |
| Unused CSS variable | 1 (`--font-geist-mono` never defined) | Low |
| Stale MSW handlers | 0 | None |
