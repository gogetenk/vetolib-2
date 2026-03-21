# todo-front-posthog-events-001 — Instrumentation des composants features

**Module** : Frontend (Analytics)
**Dependances** : todo-front-posthog-setup-001
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`
**[MSW: oui]** -- pas de dependance backend

---

## Contexte

PostHog est installe et configure (tache setup). Il faut maintenant ajouter `trackEvent()` dans les composants features pour capturer les comportements utilisateur listes dans `docs/ANALYTICS-STUDY.md` section 4.

---

## Scope

### Events P1 (obligatoires)

Ajouter `trackEvent()` dans les composants suivants. Chaque event doit inclure les properties listees.

| Composant | Event | Properties |
|-----------|-------|------------|
| `AppointmentForm.tsx` | `appointment_form_opened` | - |
| `AppointmentForm.tsx` | `appointment_created` | `species`, `has_notes` |
| `AppointmentsTable.tsx` | `appointment_status_changed` | `from_status`, `to_status` |
| `PatientForm.tsx` | `patient_created` | `species`, `has_microchip` |
| `PatientForm.tsx` (ou composant liste) | `patient_searched` | `has_results`, `query_length` |
| `MedicalRecordForm.tsx` | `medical_record_added` | `has_prescription`, `record_type` |
| `InvoiceForm.tsx` | `invoice_created` | `item_count`, `total_aed` |
| `InvoiceDetail.tsx` | `invoice_status_changed` | `from_status`, `to_status` |
| `TodayAppointments.tsx` / Dashboard | `dashboard_viewed` | `has_analytics_section` |
| Error boundary / apiFetch catch | `api_error_displayed` | `endpoint`, `status_code`, `error_code` |

### Events P2 (souhaitables)

| Composant | Event | Properties |
|-----------|-------|------------|
| `CsvImportDialog.tsx` | `patient_csv_imported` | `row_count`, `success_count`, `error_count` |
| `InvoiceDetail.tsx` | `invoice_pdf_downloaded` | - |
| Team settings (invite) | `user_invited` | `role` |
| Team settings (role change) | `user_role_changed` | `from_role`, `to_role` |
| Settings (password) | `password_changed` | - |
| `AnalyticsSection.tsx` | `analytics_section_viewed` | - |
| `LoginForm.tsx` | `form_validation_error` | `form_name: 'login'`, `field_name`, `error_type` |
| Auth flow | `session_expired` | `time_since_login` |

### Events Funnel (onboarding)

Ces events sont des alias ou des conditions speciales des events ci-dessus :

| Funnel | Sequence d'events |
|--------|-------------------|
| Onboarding | `clinic_registered` -> `first_patient_created` -> `first_appointment_created` -> `first_invoice_created` |
| Appointment flow | `appointment_form_opened` -> `appointment_created` -> `appointment_checked_in` -> `appointment_completed` |
| Billing flow | `invoice_form_opened` -> `invoice_created` -> `invoice_sent` -> `invoice_paid` |

Pour le funnel onboarding, les events `first_*` sont les memes events que `*_created` -- PostHog les differencie automatiquement via la propriete "first time" dans les funnels. Pas besoin d'events separes.

### Events AI (si les composants existent)

| Composant | Event | Properties |
|-----------|-------|------------|
| AI Triage (AppointmentForm) | `ai_triage_accepted` | `suggested_priority` |
| AI Triage (AppointmentForm) | `ai_triage_overridden` | `suggested_priority`, `chosen_priority` |

---

## Implementation

### Pattern d'appel

Utiliser `trackEvent()` de `lib/analytics.ts` (refactorise dans la tache setup) :

```typescript
import { trackEvent } from '@/lib/analytics'

// Dans le handler de succes du formulaire
const handleSubmit = async (data: FormData) => {
  const result = await createAppointment(data)
  if (result.ok) {
    trackEvent('appointment_created', {
      species: data.species,
      has_notes: String(Boolean(data.notes)),
    })
  }
}
```

### Regles pour les properties

- Toutes les properties sont des `string` (PostHog preference)
- Booleans : convertir en `"true"` / `"false"`
- Nombres : convertir en string
- NE PAS envoyer de PII (noms, emails, numeros de telephone, donnees medicales)
- NE PAS envoyer de montants exacts (pour `total_aed`, utiliser des tranches : `"0-100"`, `"100-500"`, `"500-1000"`, `"1000+"`)

### Ajouter les noms d'events dans AnalyticsEvents

Etendre le const `AnalyticsEvents` dans `lib/analytics.ts` :

```typescript
export const AnalyticsEvents = {
  // Landing page (existants, utilises par gtag)
  CTA_HERO: 'cta_click_hero',
  CTA_DEMO: 'cta_click_demo',
  // ...existants...

  // Feature usage
  APPOINTMENT_FORM_OPENED: 'appointment_form_opened',
  APPOINTMENT_CREATED: 'appointment_created',
  APPOINTMENT_STATUS_CHANGED: 'appointment_status_changed',
  PATIENT_CREATED: 'patient_created',
  PATIENT_SEARCHED: 'patient_searched',
  PATIENT_CSV_IMPORTED: 'patient_csv_imported',
  MEDICAL_RECORD_ADDED: 'medical_record_added',
  INVOICE_CREATED: 'invoice_created',
  INVOICE_STATUS_CHANGED: 'invoice_status_changed',
  INVOICE_PDF_DOWNLOADED: 'invoice_pdf_downloaded',
  USER_INVITED: 'user_invited',
  USER_ROLE_CHANGED: 'user_role_changed',
  PASSWORD_CHANGED: 'password_changed',
  DASHBOARD_VIEWED: 'dashboard_viewed',
  ANALYTICS_SECTION_VIEWED: 'analytics_section_viewed',

  // Errors
  FORM_VALIDATION_ERROR: 'form_validation_error',
  API_ERROR_DISPLAYED: 'api_error_displayed',
  SESSION_EXPIRED: 'session_expired',

  // AI
  AI_TRIAGE_ACCEPTED: 'ai_triage_accepted',
  AI_TRIAGE_OVERRIDDEN: 'ai_triage_overridden',
} as const
```

---

## Ce qui NE change PAS

- Aucune modification de logique metier dans les composants
- Aucune modification de l'API backend
- Aucune modification des handlers MSW
- Les tests Playwright existants ne doivent pas casser (trackEvent est fire-and-forget)

---

## Critere de completion

```
[] AnalyticsEvents etendu avec tous les events P1 et P2
[] trackEvent() appele dans AppointmentForm (form_opened + created)
[] trackEvent() appele dans AppointmentsTable (status_changed)
[] trackEvent() appele dans PatientForm (created + searched)
[] trackEvent() appele dans MedicalRecordForm (added)
[] trackEvent() appele dans InvoiceForm (created)
[] trackEvent() appele dans InvoiceDetail (status_changed + pdf_downloaded)
[] trackEvent() appele dans Dashboard (viewed)
[] trackEvent() appele dans CsvImportDialog (csv_imported)
[] trackEvent() appele dans le error handler API (api_error_displayed)
[] trackEvent() appele dans LoginForm (form_validation_error)
[] trackEvent() appele dans AI triage components (si existants)
[] Aucun PII dans les properties (pas de nom, email, donnees medicales)
[] Montants en tranches, pas en valeurs exactes
[] npm run build → 0 erreur
[] Tests Playwright existants toujours verts
[] Renommer en done-front-posthog-events-001.md
```
