# todo-front-fix-i18n-hardcoded-fr-001 — Remove hardcoded French strings

**Module** : Frontend
**Severity** : CRITICAL
**Skills** : `shadcn-nextjs`
**MSW** : non

---

## Problem

Several dashboard components contain hardcoded French strings instead of i18n keys.
The app targets the UAE English market — all visible text must use `useTranslations()`.

## Affected files

### `src/components/features/dashboard/StatsCards.tsx`
- Line 53: `"RDVs aujourd'hui"` → should be `t('appointments_today')`
- Line 72: `"En attente check-in"` → should be `t('pending_checkin')`
- Line 98: `"Factures impayées"` → should be `t('unpaid_invoices')`
- Line 116: `"Patients total"` → should be `t('total_patients')`
- Badge text line 85: `"urgent"` → should be `t('urgent')`

### `src/components/features/dashboard/TodayAppointments.tsx`
- Line 25–31: `STATUS_LABELS` map contains French values (`'Planifié'`, `'Arrivé'`, `'En cours'`, `'Terminé'`, `'Annulé'`) — should be English or translated via i18n
- Line 45: `formatTime` uses locale `'fr-AE'` — should use `'en-AE'`
- Line 87: Card title `"Agenda du jour"` → `t('title')` (key `dashboard.today.title`)
- Line 93: Link text `"Voir tout →"` → `t('view_all')`

### `src/components/features/billing/InvoiceTable.tsx`
- Line 124: Column header `"Subtotal HT"` → `"Subtotal"` (English, no i18n needed — already EN)

## Fix

1. Add missing keys to `src/frontend/messages/en.json` under `dashboard.stats.*` and `dashboard.today.*`
2. Add keys to `src/frontend/messages/ar.json` (Arabic equivalents)
3. Replace all hardcoded French strings with `useTranslations()` calls
4. Change `'fr-AE'` locale in `formatTime` to `'en-AE'`
5. Replace `"Subtotal HT"` with `"Subtotal (excl. VAT)"`

## Acceptance criteria

```
□ No French string remains in any TSX component
□ StatsCards uses useTranslations('dashboard.stats')
□ TodayAppointments uses useTranslations('dashboard.today')
□ formatTime locale is 'en-AE'
□ STATUS_LABELS keys are English
□ npm run build → 0 errors
```
