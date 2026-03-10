# todo-refacto-20260310-audit-004 -- data-testid manquant sur 152 boutons frontend
**Priorite** : importante
**Fichiers concernes** : 90+ fichiers .tsx dans `src/frontend/src/components/` et `src/frontend/src/app/`

Principaux fichiers (top 10 par nombre de boutons sans data-testid) :
- `components/features/billing/InvoiceForm.tsx`
- `components/features/billing/InvoiceDetail.tsx`
- `components/features/appointments/AppointmentDetail.tsx`
- `components/features/appointments/AppointmentsTable.tsx`
- `components/features/messaging/admin/TemplatesPage.tsx`
- `components/features/landing/PricingSection.tsx`
- `components/features/patients/PatientForm.tsx`
- `components/features/patients/MedicalRecordForm.tsx`
- `components/features/dashboard/TodayAppointments.tsx`
- `components/features/stock/StockTable.tsx`

**Violation** : Regle Frontend CLAUDE.md -- "data-testid obligatoire sur TOUS les elements interactifs". 152 boutons sur 165 n'ont pas de data-testid (92% de non-conformite). Seuls 13 boutons sont conformes.
**Correction attendue** : Ajouter `data-testid="action-descriptive-name"` sur chaque `<Button>` et `<button>` interactif. Convention : `data-testid="{module}-{action}"` (ex: `data-testid="billing-submit-invoice"`).
**Critere** : [] `grep -rn '<Button\|<button' src/frontend/src --include="*.tsx" | grep -v data-testid` retourne 0 resultats
