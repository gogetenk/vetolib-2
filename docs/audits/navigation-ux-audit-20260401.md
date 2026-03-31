# Navigation UX Audit — 2026-04-01

**Auditor**: UX Designer Agent
**Scope**: Dashboard navigation (sidebar, header, settings) across all roles and viewports
**Status**: Audit only -- no code changes

---

## 1. Current State of the Navigation

### 1a. Two Parallel Navigation Systems (Header + Sidebar)

The app currently ships TWO different navigation structures that are NOT synchronized:

**Desktop Header** (`Header.tsx`, lines 31-38) -- top horizontal tabs:
- Dashboard, Appointments, Messages, Patients, Billing, Stock
- 6 items, flat list, NO role filtering, NO breeding

**Mobile Sidebar** (`Sidebar.tsx`, lines 36-96) -- slide-out sheet with vertical list:
- Dashboard, Appointments, Patients, Billing, Messages, Breeding, Stock, Team, Messaging Settings
- 9 items, role-filtered (breeding/stock = VET+ADMIN, team/messaging settings = ADMIN only)
- Plus bottom section: Settings, Profile

**Critical inconsistency**: The desktop header shows Stock to ALL roles (no filtering). The sidebar correctly filters it to VET+ADMIN. A RECEPTIONIST on desktop sees Stock in the header tabs but would not see it in the mobile sidebar.

**Critical inconsistency**: Breeding is MISSING from the desktop header entirely. A VET on desktop has no way to reach `/breeding` from the top nav -- they must know the URL or use mobile.

### 1b. Item Count -- Current vs Planned

Currently in the sidebar: 9 main items + 2 bottom items = 11 total.

Features mentioned as planned/in-progress but NOT yet in navigation:
- AI Health Alerts (exists as dashboard widget `HealthAlertPanel`, no dedicated page)
- Analytics Dashboard (exists as dashboard section `AnalyticsSection`, no standalone page)
- Waitlist (no page found)
- Feedback (no page found)
- Notification Center (settings page exists at `/settings/notifications`, no inbox-style center)
- Staff Scheduling (no page found)
- Audit Trail (no page or settings section found)
- Reminders settings (no dedicated page found)

If all planned features get their own nav entry, we would reach 15-17 items. That is far too many for a flat list -- cognitive overload guaranteed, especially for a vet standing with gloves on.

---

## 2. Role-Based Filtering

### What exists

The sidebar has a `roles` property on `NavItem`. It works correctly:
- `undefined` roles = visible to all
- `["VET", "ADMIN"]` = breeding, stock
- `["ADMIN"]` = team, messaging settings

### What is missing

- **Desktop header has ZERO role filtering.** The `navItems` array in `Header.tsx` (lines 31-38) is a plain array with no `roles` field. Every role sees every tab.
- **No module-level toggling.** There is no concept of "this clinic has breeding enabled" vs "this clinic does not use breeding." Every VET/ADMIN sees breeding whether or not they need it.
- **No `RECEPTIONIST`-specific view.** A receptionist's primary workflow is: check-in patients, manage appointments, handle billing. They currently see the same flat nav as a vet (on desktop).

---

## 3. Orphan Pages (accessible by URL, absent from nav)

| Route | In Header? | In Sidebar? | Reachable how? |
|---|---|---|---|
| `/dashboard` | Yes | Yes | OK |
| `/appointments` | Yes | Yes | OK |
| `/patients` | Yes | Yes | OK |
| `/billing` | Yes | Yes | OK |
| `/messages` | Yes | Yes | OK |
| `/stock` | Yes | Yes | OK |
| `/breeding` | **NO** | Yes (VET/ADMIN) | Desktop: orphan |
| `/profile` | No | Yes (bottom) | Desktop: only via UserMenu or sidebar |
| `/settings` | No (icon only) | Yes (bottom) | Desktop: gear icon in header |
| `/settings/preferences` | No | No | Only via Settings index page |
| `/settings/team` | No | Yes (sidebar) | Duplicated: sidebar + settings page |
| `/settings/messaging/*` | No | Yes (sidebar, ADMIN) | Duplicated |
| `/settings/notifications` | No | No | Only via Settings index page |
| `/settings/working-hours` | No | No | Only via Settings index page |
| `/patients/[id]/records/new` | No | No | Only via patient detail |
| `/stock/drugs`, `/stock/history` | No | No | Only via stock page tabs |

**3 pages are truly orphaned on desktop** (no nav path without knowing the URL):
1. `/breeding` -- missing from header
2. `/settings/notifications` -- only reachable via settings index
3. `/settings/working-hours` -- only reachable via settings index

The settings sub-pages are acceptable (hub-and-spoke pattern). But `/breeding` being absent from the desktop header is a bug.

---

## 4. Mobile Behavior

### What works well
- The sidebar opens as a `Sheet` (slide-over), which is the correct pattern for mobile nav.
- Items have `onItemClick` to close the sheet after navigation.
- The hamburger button is hidden on `lg:` breakpoint and above.
- RTL support is present (`ltr:/rtl:` classes).

### What is concerning
- **11 items in a vertical list on a 375px screen.** With the current item height (~44px each), the list takes approximately 484px -- nearly the full viewport height. Adding more features will require scrolling inside the sheet, which is awkward when you are standing and holding a phone with one hand.
- **No grouping or visual hierarchy.** All 9 main items look identical. There is no separator between "clinical" items (appointments, patients, breeding) and "admin" items (billing, stock, team, messaging settings).
- **The bottom section (Settings, Profile) is pushed below the fold** if the main list grows further.
- **No search/command palette.** On desktop, power users (admins managing multiple things) have no quick-jump mechanism.

---

## 5. i18n Completeness for Nav Keys

All 3 languages (en, fr, ar) have the same nav keys:
- dashboard, appointments, patients, medical_records, billing, messages, team, settings, profile, sign_out, stock, breeding, read_only, messaging_settings, switch_clinic, select_clinic, your_clinics

**No missing keys detected** for navigation labels. This is good.

However, the breeding page (`breeding/page.tsx`) has hardcoded English strings:
- "Breeding Dashboard" (line 102)
- "Active Pregnancies" (line 118, 149)
- "Recent Litters" (line 196)
- "Recent Heat Cycles" (line 235)
- Column headers are all hardcoded English

This means the breeding page is NOT internationalized even though the nav label is. A vet using the Arabic UI will click an Arabic nav label and land on an all-English page.

---

## 6. Recommendations

### CRITICAL -- Fix immediately

**C1. Sync header and sidebar nav items.**
The desktop header (`Header.tsx`) must use the same `getMainNavItems()` function from `Sidebar.tsx` (or a shared source of truth). Currently they are two independent arrays that drift apart. This is the root cause of breeding being missing on desktop and role filtering being absent on desktop.

Proposed approach: extract a single `navConfig.ts` that both `Header.tsx` and `Sidebar.tsx` consume. Each item declares: href, label key, icon, roles, and whether it appears in the "compact" header (top 6) vs only in the full sidebar.

**C2. Add role filtering to the desktop header.**
Stock should not be visible to RECEPTIONIST/ASSISTANT on desktop. Breeding should be visible to VET/ADMIN. Use the same `roles` property already defined in the sidebar.

### IMPORTANT -- Before adding more features

**I1. Group navigation items by domain.**
As the feature count grows beyond 8, introduce lightweight section headers in the sidebar:

```
-- Clinical --
  Dashboard
  Appointments
  Patients
  Breeding (if enabled)

-- Operations --
  Billing
  Stock
  Messages

-- Admin -- (ADMIN role only)
  Team
  Settings
```

On the desktop header, keep only the top 5-6 most-used items as tabs. Put the rest under a "More" dropdown or rely on the settings hub for admin items.

**I2. Introduce module-level feature flags.**
Not every clinic uses breeding. Not every clinic manages stock. The sidebar should respect a `clinic.enabledModules` array so that irrelevant items disappear. This reduces noise for clinics that only do consultations.

**I3. Add a command palette (Cmd+K / Ctrl+K).**
For power users (especially admins), a search-based quick nav is faster than scanning 10+ items. shadcn/ui has a `CommandDialog` component ready to use. This also solves the "orphan page" problem -- any page can be found via search even if it is not in the sidebar.

### NICE-TO-HAVE -- Future improvements

**N1. Collapsible sidebar on desktop.**
If the app moves to a sidebar layout on desktop (instead of the current top-tabs), consider a collapsible icon-only mode for wider content area. This is standard in tools like Linear, Notion, etc.

**N2. Contextual quick actions.**
Instead of adding more nav items for things like "New Appointment" or "New Patient", surface contextual action buttons on the relevant pages. The sidebar should be for navigation, not for actions.

**N3. Badge consolidation.**
Currently only Messages has an unread badge. If Notification Center and Waitlist get nav entries, define a consistent badge pattern now (color, position, animation) before it fragments.

**N4. Internationalize the breeding page.**
All hardcoded English strings in `breeding/page.tsx` should use `useTranslations("breeding")` with keys in all 3 language files.

---

## 7. Summary Matrix

| Issue | Severity | Effort | Recommendation |
|---|---|---|---|
| Header and sidebar nav are desynchronized | Critical | Small | C1: shared navConfig |
| Desktop header has no role filtering | Critical | Small | C2: apply roles filter |
| Breeding missing from desktop header | Critical | Tiny | Fixed by C1 |
| No nav grouping (flat list of 9+ items) | Important | Medium | I1: section headers |
| No module-level feature flags in nav | Important | Medium | I2: clinic.enabledModules |
| No command palette for quick navigation | Important | Medium | I3: Cmd+K dialog |
| Breeding page not internationalized | Important | Small | N4: add i18n keys |
| No collapsible sidebar option | Nice-to-have | Large | N1: collapsible mode |
| No contextual quick actions | Nice-to-have | Medium | N2: page-level CTAs |
| Badge pattern not defined for future features | Nice-to-have | Small | N3: design badge system |

---

## 8. Wireframe -- Proposed Sidebar with Groups

```
+----------------------------------+
| [paw] Vetara    [clinic switcher]|
+----------------------------------+
|                                  |
|  CLINICAL                        |
|  [icon] Dashboard                |
|  [icon] Appointments        (3)  |
|  [icon] Patients                 |
|  [icon] Breeding *               |
|                                  |
|  OPERATIONS                      |
|  [icon] Messages           (12)  |
|  [icon] Billing                  |
|  [icon] Stock *                  |
|                                  |
|  -------- (ADMIN only) -------- |
|  ADMIN                           |
|  [icon] Team                     |
|  [icon] Settings                 |
|                                  |
|  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~    |
|  [avatar] Dr. Ahmed    [v]       |
+----------------------------------+

* = visible only if module enabled for this clinic
(3) = appointment count today
(12) = unread messages
```

For the desktop header (lg+ breakpoint), keep the top 6 tabs:
```
[Vetara] [ClinicSwitcher]  Dashboard | Appointments | Patients | Messages | Billing | [More v]  [gear] [avatar]
                                                                                        |
                                                                                  Stock
                                                                                  Breeding
```

The "More" dropdown contains items that overflow the tab bar. This keeps the header clean while giving access to everything.

---

**Next step**: Submit this audit to PO for validation before any implementation begins. The critical items (C1, C2) should be addressed in the next sprint as they represent real user-facing bugs (features unreachable on certain viewports/roles).
