# todo-front-agenda-calendar-003.md -- Quick appointment creation from calendar

**Module** : Frontend / Agenda
**Dependances** : todo-front-agenda-calendar-001 (week view must exist)
**Priorite** : HAUTE
[MSW: oui] -- developpement sans backend requis
**Skills a lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Objectif

Permettre la **creation rapide d'un rendez-vous** en cliquant sur un creneau vide du calendrier. Un clic ouvre un formulaire allege pre-rempli avec la date et l'heure du creneau. Ajouter aussi un **panel lateral** (Sheet) pour visualiser le detail d'un RDV existant sans quitter le calendrier.

---

## Perimetre exact

### Fichiers a creer / modifier

- `components/features/calendar/QuickAppointmentForm.tsx` -- formulaire allege dans un Dialog
- `components/features/calendar/AppointmentDetailSheet.tsx` -- panel lateral detail RDV
- `components/features/calendar/WeekCalendar.tsx` -- ajouter clic sur creneau vide + clic sur RDV -> sheet
- `components/features/calendar/DayCalendar.tsx` -- idem
- `components/features/calendar/MonthCalendar.tsx` -- clic sur "+N more" -> liste, clic sur RDV -> sheet
- `mocks/handlers/appointments.ts` -- s'assurer que POST /api/appointments accepte consultationType
- `e2e/calendar/calendar-quick-create.spec.ts` -- tests Playwright

---

## Spec UX : Clic sur creneau vide

### Comportement

1. L'utilisateur clique sur une zone vide du calendrier (entre deux RDV, ou un creneau libre)
2. Un **Dialog** s'ouvre avec un formulaire allege :
   - **Date** : pre-remplie (jour de la colonne cliquee) -- readonly, affichee comme label
   - **Heure** : pre-remplie (creneau clique, arrondi au quart d'heure le plus proche) -- editable
   - **Patient** : champ texte avec autocomplete (recherche parmi les patients existants) -- pour le MVP, un simple champ texte suffit
   - **Owner** : champ texte
   - **Consultation type** : Select avec les types de consultation (couleur visible dans le select)
   - **Veterinaire** : Select avec les vets disponibles
   - **Raison** : Textarea court (2 lignes)
3. Boutons : "Create" (primary) + "Cancel" (ghost) + "Full form" (lien vers /appointments/new avec pre-fill en query params)

### Regle metier

- On ne peut PAS creer de RDV sur un creneau deja occupe (le clic sur un bloc existant ouvre le detail, pas la creation)
- On ne peut PAS creer de RDV en dehors des heures d'ouverture (les zones grisees ne sont pas cliquables)
- On ne peut PAS creer de RDV sur vendredi/samedi si la clinique est fermee ces jours-la (zones grisees = non cliquables)

### Apres creation

- Le Dialog se ferme
- Le nouveau RDV apparait immediatement dans le calendrier (optimistic update ou refetch)
- Toast de confirmation : "Appointment created for {patientName} at {time}"

---

## Spec UX : Panel lateral detail RDV (AppointmentDetailSheet)

### Comportement

1. L'utilisateur clique sur un **bloc RDV existant** dans le calendrier
2. Un **Sheet** (panel lateral droite, shadcn Sheet) s'ouvre avec :

```
+----------------------------------+
| [X]  Appointment Detail          |
+----------------------------------+
| Patient: Max (Dog)               |
| Owner: Ahmed Al-Rashid           |
| Phone: +971 50 123 4567          |
+----------------------------------+
| Type: [Vaccination]  (badge)     |
| Vet: Dr. Sarah Johnson           |
| Duration: 30 min                 |
+----------------------------------+
| Date: Sunday, March 12, 2026    |
| Time: 09:00 - 09:30              |
| Status: [SCHEDULED] (badge)      |
+----------------------------------+
| Reason:                          |
| Annual vaccination               |
+----------------------------------+
| Notes:                           |
| Owner requested morning slot     |
+----------------------------------+
| [Check In]  [Cancel]             |
| [View full details ->]           |
+----------------------------------+
```

### Transitions de statut

Les memes boutons que la page detail existante :

| Statut actuel | Boutons disponibles |
|---|---|
| SCHEDULED | "Check In" + "Cancel" |
| CHECKED_IN | "Start Consultation" |
| IN_PROGRESS | "Complete" |
| COMPLETED | aucun |
| CANCELLED | aucun |

Chaque transition : Dialog de confirmation avant execution.

### Lien "View full details"

Navigue vers `/appointments/{id}` (page detail complete existante).

---

## Indicateur visuel de creneau cliquable

- Au **hover sur un creneau vide** : fond legerement colore (blue-50 / primary-50) + cursor pointer + icone "+" discrete au centre
- Au **hover sur un bloc RDV** : shadow-md + cursor pointer
- Les zones hors-ouverture et les jours fermes : cursor not-allowed, pas de hover effect

---

## RTL support

- Le Sheet s'ouvre depuis la **gauche** en RTL (pas la droite)
- Le formulaire de creation rapide est aligne a droite
- Les boutons de transition sont inverses (primary a gauche en RTL)

---

## i18n (chaines supplementaires)

- `calendar.quickCreate.title` : "New Appointment"
- `calendar.quickCreate.submit` : "Create"
- `calendar.quickCreate.cancel` : "Cancel"
- `calendar.quickCreate.fullForm` : "Open full form"
- `calendar.quickCreate.success` : "Appointment created for {name} at {time}"
- `calendar.detail.title` : "Appointment Detail"
- `calendar.detail.viewFull` : "View full details"
- `calendar.detail.patient`, `calendar.detail.owner`, etc.
- `calendar.closedSlot` : "Clinic is closed at this time"

---

## data-testid obligatoires

- `calendar-slot-{date}-{time}` -- chaque creneau cliquable (format: YYYY-MM-DD-HHmm)
- `quick-create-dialog` -- le dialog de creation rapide
- `quick-create-patient-input` -- champ patient
- `quick-create-owner-input` -- champ owner
- `quick-create-type-select` -- select type de consultation
- `quick-create-vet-select` -- select veterinaire
- `quick-create-reason-input` -- textarea raison
- `quick-create-submit-btn` -- bouton creer
- `quick-create-cancel-btn` -- bouton annuler
- `quick-create-full-form-link` -- lien vers formulaire complet
- `appointment-detail-sheet` -- le panel lateral
- `detail-sheet-checkin-btn`, `detail-sheet-cancel-btn`, `detail-sheet-start-btn`, `detail-sheet-complete-btn` -- boutons transition
- `detail-sheet-view-full-link` -- lien vers page detail

---

## Critere de completion

```
[ ] Clic sur creneau vide ouvre le dialog de creation rapide
[ ] Date et heure sont pre-remplies selon le creneau clique
[ ] Le formulaire allege permet de creer un RDV
[ ] Le RDV apparait immediatement dans le calendrier apres creation
[ ] Toast de confirmation affiche
[ ] Creneaux hors-ouverture et jours fermes non cliquables
[ ] Clic sur un RDV existant ouvre le Sheet lateral
[ ] Le Sheet affiche toutes les infos du RDV
[ ] Les transitions de statut fonctionnent depuis le Sheet
[ ] Lien "View full details" navigue vers la page detail
[ ] Hover sur creneau vide montre l'indicateur "+"
[ ] RTL : Sheet s'ouvre depuis la gauche, formulaire aligne correctement
[ ] i18n : toutes les chaines traduites EN + AR
[ ] data-testid presents sur tous les elements
[ ] Tests Playwright passent
[ ] npm run build : 0 erreurs
[ ] Renommer en done-front-agenda-calendar-003.md
```
