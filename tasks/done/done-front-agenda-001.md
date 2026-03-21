# todo-front-agenda-001.md — Frontend : Agenda / Rendez-vous

**Module** : Frontend / Agenda
**Dépendances** : front-scaffold-000, back-agenda-001
**Gherkins** : `features/agenda/appointments.feature` (scénarios UI)
[MSW: oui] — développement sans backend requis
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Périmètre exact

- `app/(dashboard)/appointments/page.tsx` — liste + bouton "New"
- `app/(dashboard)/appointments/new/page.tsx` — formulaire création
- `app/(dashboard)/appointments/[id]/page.tsx` — détail + transitions statut
- `components/features/appointments/AppointmentsTable.tsx`
- `components/features/appointments/AppointmentForm.tsx`
- `components/features/appointments/StatusBadge.tsx`
- `lib/api/appointments.ts`
- `e2e/appointments/appointments.spec.ts`

---

## Liste des rendez-vous

Tableau DataTable (tanstack/react-table) avec colonnes :
| Date/Heure | Patient | Owner | Vétérinaire | Statut | Actions |
|---|---|---|---|---|---|

- Tri par date (défaut : aujourd'hui en premier)
- Filtre par statut (Select shadcn)
- Filtre par date (DatePicker shadcn)
- Pagination (10 par page)
- Bouton "New Appointment" (haut droit)
- `data-testid` : "appointments-table", "new-appointment-btn", "status-filter"

---

## Formulaire création

Champs :
- Patient Name (text)
- Species (Select : Dog, Cat, Bird, Rabbit, Horse, Exotic)
- Owner Name (text)
- Owner Phone (text)
- Vétérinaire assigné (Select — liste depuis API)
- Date (DatePicker — pas de weekend selon config clinique)
- Heure (Select — créneaux de 30min, 08:00 → 19:00)
- Motif (Textarea)
- Notes (Textarea, optionnel)

---

## Détail + transitions

Affiche toutes les infos + boutons de transition selon le statut actuel :

| Statut actuel | Boutons disponibles |
|---|---|
| SCHEDULED | "Check In" → CHECKED_IN, "Cancel" → CANCELLED |
| CHECKED_IN | "Start Consultation" → IN_PROGRESS |
| IN_PROGRESS | "Complete" → COMPLETED |
| COMPLETED | (aucun) |
| CANCELLED | (aucun) |

Chaque bouton : Dialog de confirmation avant action.

---

## lib/api/appointments.ts

```typescript
export async function getAppointments(filters?: AppointmentFilters): Promise<PagedResult<AppointmentDto>>
export async function getAppointment(id: string): Promise<AppointmentDto>
export async function createAppointment(data: CreateAppointmentRequest): Promise<AppointmentDto>
export async function transitionAppointment(id: string, action: AppointmentAction): Promise<AppointmentDto>
export async function cancelAppointment(id: string, reason: string): Promise<AppointmentDto>
```

---

## Critère de complétion

```
□ Liste des rendez-vous s'affiche avec pagination
□ Création d'un RDV fonctionne
□ Transitions de statut fonctionnent
□ Tests Playwright passent
□ Renommer en done-front-agenda-001.md
```
