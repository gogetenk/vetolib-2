# todo-front-fix-sidebar-logo-001 — Add logo/clinic header to desktop Sidebar

**Module** : Frontend
**Severity** : MEDIUM
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

The desktop sidebar (`Sidebar.tsx`) starts directly with nav items — there is no logo or
clinic name at the top. The header bar shows the logo, but on wide screens users typically
look at the sidebar top for orientation.

The mobile Sheet sidebar correctly shows the logo in `SheetTitle`. The desktop sidebar
should have the same visual anchor.

Current layout:
```
┌─────────────────┐
│ [nav items ...]  │  ← starts immediately, no logo
```

Expected layout:
```
┌─────────────────┐
│ 🐾 Vetolib       │  ← logo + clinic name (same as header Logo + clinicName)
├─────────────────┤
│ [nav items ...]  │
```

## Fix

In `src/components/features/shell/Sidebar.tsx`:

1. Add a logo header section at the top of the desktop `<aside>`:
```tsx
<div className="flex h-16 items-center border-b px-4">
  <Link
    href="/appointments"
    data-testid="sidebar-logo"
    className="flex items-center gap-2 font-bold text-lg text-primary"
  >
    <PawPrint className="h-6 w-6" />
    <span>Vetolib</span>
  </Link>
</div>
```

2. The `h-16` height matches the Header bar height exactly, creating a continuous top bar.

3. Import `PawPrint` from `lucide-react` and `Link` from `next/link` (already present in the file).

4. Do NOT add clinic name in the sidebar — it is already shown in the Header `clinic-name` span.
   Adding it in both places creates redundancy.

## Acceptance criteria

```
□ Desktop sidebar shows PawPrint + "Vetolib" logo at the top
□ Logo height (h-16) aligns with Header height
□ Logo is a Link to /appointments
□ data-testid="sidebar-logo" present
□ Mobile Sheet sidebar unchanged (it already has SheetTitle)
□ npm run build → 0 errors
```
