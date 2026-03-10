# todo-front-preferences-page-001.md -- Settings Preferences page

**Module** : Frontend (Preferences)
**Dependances** : todo-back-preferences-api-001 (contrats API -- MSW suffit)
**Priorite** : MOYENNE
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`
[MSW: oui]
[Branchement ulterieur: todo-wire-preferences-001]

---

## Objectif

Creer la page Settings/Preferences avec toggles par categorie, affichage des defaults clinique vs overrides user, et RBAC (section admin visible uniquement pour les Admins).

## Spec de reference

- `docs/PREFERENCES-STUDY.md` section 6 (Frontend)
- `docs/PO-POST-MVP-DECISIONS.md` section 2

## Decisions PO a respecter

- Drug interaction alerts : toggle affiche mais **disabled + tooltip** "This safety feature cannot be disabled"
- AI features : visibles par tous, modifiables uniquement par Admin (griser les toggles pour les non-admins avec mention "Contact your clinic admin")
- Chaque toggle affiche la source de la valeur (system default, clinic default, user override) en texte discret

## Implementation

### Page

`src/frontend/src/app/[locale]/(dashboard)/settings/preferences/page.tsx`

### Structure UI

Quatre sections collapsibles (Accordion shadcn/ui) :

1. **Notifications** (per-user)
   - Email notifications : toggle
   - Push notifications : toggle (disabled, "Coming soon")
   - SMS notifications : toggle (disabled, "Coming soon")
   - Appointment reminders : toggle
   - Invoice notifications : toggle

2. **AI Features** (per-clinic, Admin only modifiable)
   - AI Triage suggestions : toggle (Admin modifiable, others read-only)
   - No-show predictions : toggle (Admin modifiable, others read-only)
   - AI messaging assistance : toggle (Admin modifiable, others read-only)
   - Drug interaction alerts : toggle (disabled, always ON, tooltip)

3. **Privacy & Analytics** (mixed)
   - Analytics tracking : toggle (Admin only)
   - Usage data collection : toggle (Admin only)
   - Cross-clinic data sharing : toggle (per-user)
   - Marketing emails : toggle (per-user)

4. **Communication** (per-user)
   - Quiet hours start : time picker
   - Quiet hours end : time picker
   - Preferred channel : select (Email / SMS / Push)
   - Language : select (English / Arabic)

### Composants

- `PreferencesPage` : page principale
- `PreferenceCategorySection` : section collapsible avec titre et description
- `PreferenceToggle` : toggle individuel avec label, source badge, disabled state
- `PreferenceSelect` : select pour les valeurs non-booleennes
- `PreferenceTimePicker` : time picker pour quiet hours

### Hooks

- `usePreferences()` : GET /api/preferences, retourne les preferences groupees par categorie
- `useUpdatePreference(key)` : PUT /api/preferences/{key}, mutation optimiste
- `useBulkUpdatePreferences()` : PUT /api/preferences, pour "Save all"
- `useClinicDefaults()` : GET /api/clinics/preferences (Admin only)
- `useUpdateClinicDefaults()` : PUT /api/clinics/preferences (Admin only)

### API client

`src/frontend/src/lib/api/preferences.ts` :
- `getPreferences(): Promise<PreferenceCategoryDto[]>`
- `updatePreference(key: string, value: string): Promise<void>`
- `bulkUpdatePreferences(prefs: {key: string, value: string}[]): Promise<void>`
- `revokeConsent(category: string): Promise<void>`
- `getClinicDefaults(): Promise<PreferenceCategoryDto[]>`
- `updateClinicDefaults(defaults: {key: string, value: string}[]): Promise<void>`

### MSW handlers

`src/frontend/src/mocks/handlers/preferences.ts` :
- Mock GET /api/preferences avec donnees realistes (noms UAE, timezone Asia/Dubai)
- Mock PUT /api/preferences/{key} avec validation AIDrugInteractions
- Mock GET /api/clinics/preferences
- Mock PUT /api/clinics/preferences
- Mock POST /api/preferences/consent/revoke

### data-testid obligatoires

- `data-testid="preferences-page"`
- `data-testid="pref-section-Notifications"`
- `data-testid="pref-section-AIFeatures"`
- `data-testid="pref-section-Privacy"`
- `data-testid="pref-section-Communication"`
- `data-testid="pref-toggle-{PreferenceKey}"` sur chaque toggle (ex: `pref-toggle-NotificationEmail`)
- `data-testid="pref-select-CommunicationLanguage"`
- `data-testid="pref-source-badge-{PreferenceKey}"` sur chaque badge source
- `data-testid="pref-save-btn"`
- `data-testid="pref-revoke-analytics-btn"`
- `data-testid="pref-drug-interaction-tooltip"`

### i18n

Ajouter les cles dans `src/frontend/messages/en.json` et `src/frontend/messages/ar.json` :
- `preferences.title`, `preferences.notifications.title`, etc.
- Tous les labels de preference
- Messages d'erreur ("Drug interaction alerts cannot be disabled")

### Tests Playwright

`src/frontend/e2e/integration/preferences.spec.ts` :

```
- User sees all preference categories
- User toggles a notification preference ON/OFF
- User sees source badge (system_default, clinic_default, user_override)
- Drug interaction toggle is disabled with tooltip
- Non-admin sees AI toggles as read-only with "Contact your clinic admin"
- Admin can modify AI feature toggles
- User revokes analytics consent -> all analytics toggles go to OFF
- User changes language preference
- User changes quiet hours
- Page persists changes after reload
```

## Critere

```
[] Page /[locale]/settings/preferences accessible
[] 4 sections collapsibles (Notifications, AI, Privacy, Communication)
[] Toggles fonctionnels avec mutation optimiste
[] Source badge affiche sur chaque preference
[] Drug interaction toggle disabled + tooltip
[] AI toggles read-only pour non-admins
[] Admin peut modifier les clinic defaults
[] Bouton "Revoke analytics consent" fonctionnel
[] MSW handlers complets dans src/mocks/handlers/preferences.ts
[] API client dans src/lib/api/preferences.ts
[] data-testid sur tous les elements interactifs
[] i18n EN + AR
[] Tests Playwright verts
[] Renommer en done
```
