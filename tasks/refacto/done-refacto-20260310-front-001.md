# todo-refacto-20260310-front-001 — Boutons sans data-testid dans le frontend
**Priorite** : importante
**Fichiers concernes** (liste non exhaustive, 86 occurrences) :
- `src/frontend/src/components/features/appointments/AppointmentForm.tsx` (8 boutons)
- `src/frontend/src/components/features/appointments/AppointmentDetail.tsx` (4 boutons)
- `src/frontend/src/components/features/appointments/AppointmentsTable.tsx` (4 boutons)
- `src/frontend/src/components/features/billing/InvoiceForm.tsx` (6 boutons)
- `src/frontend/src/components/features/billing/InvoiceDetail.tsx` (5 boutons)
- `src/frontend/src/components/features/patients/PatientForm.tsx` (2 boutons)
- `src/frontend/src/components/features/patients/MedicalRecordForm.tsx` (2 boutons)
- `src/frontend/src/components/features/patients/CsvImportDialog.tsx` (2 boutons)
- `src/frontend/src/components/features/users/TeamTable.tsx` (2 boutons)
- `src/frontend/src/components/features/users/InviteUserDialog.tsx` (4 boutons)
- `src/frontend/src/components/features/users/ChangeRoleDialog.tsx` (2 boutons)
- `src/frontend/src/components/features/stock/StockItemForm.tsx` (2 boutons)
- `src/frontend/src/components/features/stock/StockMovementForm.tsx` (2 boutons)
- `src/frontend/src/components/features/stock/StockTable.tsx` (2 boutons)
- `src/frontend/src/components/features/landing/*.tsx` (6+ boutons)
- `src/frontend/src/components/features/shell/UserMenu.tsx` (2 boutons)
- `src/frontend/src/components/features/auth/LoginForm.tsx` (1 bouton)
- `src/frontend/src/components/features/auth/SignupForm.tsx` (1 bouton)
**Violation** : Regle frontend de CLAUDE.md — `data-testid` obligatoire sur TOUS les elements interactifs. Environ 86 boutons dans les composants features n'ont pas de `data-testid`, ce qui bloque les tests Playwright.
**Correction attendue** : Ajouter `data-testid` unique a chaque `<Button>` et `<button>` dans les composants features (hors `components/ui/` qui sont des primitives generiques).
**Critere** : `grep -r "<Button\|<button" src/frontend/src --include="*.tsx" | grep -v "data-testid" | grep -v "components/ui/"` retourne 0 resultats
