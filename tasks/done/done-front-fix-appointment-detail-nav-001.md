# todo-front-fix-appointment-detail-nav-001 — Fix navigation hierarchy in AppointmentDetail

**Module** : Frontend
**Severity** : MEDIUM
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

`AppointmentDetail.tsx` has two separate action zones at the bottom of the card:
1. A `transition-actions` div with the primary action buttons (Check In, Cancel, Start, etc.)
2. A second isolated `div` directly below with a lone "Back to list" button

This creates confusing visual hierarchy: the user has to scroll past action buttons to find
navigation. It also diverges from the patient detail page which uses a back arrow at the
**top** of the page (consistent with standard app navigation patterns).

Current layout (bottom of card):
```
[ Check In ]  [ Cancel ]     ← border-t section
[ Back to list ]             ← another isolated div
```

The patient detail page (`/patients/[id]/page.tsx`) uses:
```tsx
<Button render={<Link href="../patients" />} variant="ghost" size="sm">
  <ArrowLeft className="h-4 w-4 me-1" />
  Appointments
</Button>
```
at the **top** of the page, before the content card.

## Fix

In `src/app/[locale]/(dashboard)/appointments/[id]/page.tsx` (or the loader component):

1. Add a back button at the **top** of the page (before the `<AppointmentDetail>` card):
```tsx
<Button
  render={<Link href="../appointments" />}
  variant="ghost"
  size="sm"
  data-testid="back-to-appointments-btn"
  className="-ms-2"
>
  <ArrowLeft className="h-4 w-4 me-1" />
  Appointments
</Button>
```

2. Remove the `btn-back` / "Back to list" button from the bottom of `AppointmentDetail.tsx`
   (lines 182–190).

3. The transition action buttons (Check In, Cancel, etc.) remain inside the card as-is.

## Acceptance criteria

```
□ Back button (ArrowLeft + "Appointments") appears at top of the page, above the card
□ data-testid="back-to-appointments-btn" present
□ "Back to list" btn-back removed from AppointmentDetail card bottom
□ Transition action buttons (btn-action-*) unchanged inside the card
□ npm run build → 0 errors
```
