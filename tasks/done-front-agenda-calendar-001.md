# todo-front-agenda-calendar-001.md -- Calendar component: week view (default)

**Module** : Frontend / Agenda
**Dependances** : aucune
**Priorite** : HAUTE (ecran principal du veterinaire)
[MSW: oui] -- developpement sans backend requis
**Skills a lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Objectif

Remplacer la liste de rendez-vous actuelle (`AppointmentsTable`) par un **composant calendrier professionnel en vue semaine** (vue par defaut). Le calendrier affiche les rendez-vous sous forme de blocs colores par type de consultation, avec navigation temporelle et filtre par veterinaire.

---

## Perimetre exact

### Fichiers a creer / modifier

- `components/features/calendar/WeekCalendar.tsx` -- composant calendrier semaine
- `components/features/calendar/CalendarHeader.tsx` -- navigation (prev/next/today) + toggle vues + filtre vet
- `components/features/calendar/AppointmentBlock.tsx` -- bloc individuel d'un RDV dans le calendrier
- `components/features/calendar/TimeColumn.tsx` -- colonne des heures (07:00 - 21:00)
- `components/features/calendar/types.ts` -- types TypeScript pour le calendrier
- `components/features/calendar/consultation-colors.ts` -- mapping type de consultation -> couleur
- `app/[locale]/(dashboard)/appointments/page.tsx` -- remplacer AppointmentsTable par WeekCalendar
- `mocks/handlers/appointments.ts` -- enrichir les mock data avec `consultationType`
- `lib/api/appointments.ts` -- ajouter `consultationType` au type `AppointmentDto`
- `e2e/calendar/calendar-week.spec.ts` -- tests Playwright

### Fichiers a ne PAS modifier

- `app/[locale]/(dashboard)/appointments/new/page.tsx` -- formulaire creation (inchange)
- `app/[locale]/(dashboard)/appointments/[id]/page.tsx` -- detail (inchange pour cette tache)
- `components/features/appointments/AppointmentsTable.tsx` -- conserve mais plus utilise comme vue par defaut

---

## Spec UX detaillee

### Layout general

```
+------------------------------------------------------------------+
| < Prev  [Today]  Next >     [Day] [*Week*] [Month]   [Vet: All] |
+------------------------------------------------------------------+
|        | Sun 12  | Mon 13  | Tue 14  | Wed 15  | Thu 16  | ...   |
| 07:00  |         |         |         |         |         |       |
| 07:30  |         |         |         |         |         |       |
| 08:00  |  [====] |         |         |         |         |       |
| 08:30  |         |  [====] |         |         |         |       |
| ...    |         |         |         |         |         |       |
| 20:30  |         |         |         |         |         |       |
| 21:00  |         |         |         |         |         |       |
+------------------------------------------------------------------+
```

### Bloc RDV (AppointmentBlock)

Chaque rendez-vous est un bloc positionne verticalement selon l'heure, avec hauteur proportionnelle a la duree :
- **Hauteur** : `(durationMinutes / 30) * rowHeight`
- **Couleur de fond** : couleur du type de consultation (opacite 20%) + bordure gauche 3px (couleur pleine)
- **Contenu** :
  - Ligne 1 : Nom du patient + emoji espece (petit, inline)
  - Ligne 2 : Nom du proprietaire (text-xs, text-muted)
  - Ligne 3 : Type de consultation (badge petit)
- **Badge statut** : coin haut-droit, petit cercle colore (vert=in progress, jaune=checked in, bleu=scheduled, gris=completed, rouge=cancelled)
- **Hover** : elevation (shadow-md), cursor pointer
- **Clic** : navigue vers `/appointments/{id}` (detail) -- pour cette tache. La tache 003 ajoutera le panel lateral.

### Couleurs par type de consultation

```typescript
// consultation-colors.ts
export const CONSULTATION_COLORS: Record<string, { bg: string; border: string; text: string }> = {
  'General Checkup':       { bg: 'bg-blue-100',    border: 'border-l-blue-500',    text: 'text-blue-700' },
  'Vaccination':           { bg: 'bg-green-100',   border: 'border-l-green-500',   text: 'text-green-700' },
  'Surgery':               { bg: 'bg-red-100',     border: 'border-l-red-500',     text: 'text-red-700' },
  'Emergency':             { bg: 'bg-orange-100',  border: 'border-l-orange-500',  text: 'text-orange-700' },
  'Dental':                { bg: 'bg-purple-100',  border: 'border-l-purple-500',  text: 'text-purple-700' },
  'Dermatology':           { bg: 'bg-pink-100',    border: 'border-l-pink-500',    text: 'text-pink-700' },
  'Follow-up':             { bg: 'bg-teal-100',    border: 'border-l-teal-500',    text: 'text-teal-700' },
  'Grooming':              { bg: 'bg-amber-100',   border: 'border-l-amber-500',   text: 'text-amber-700' },
  'Laboratory / Diagnostics': { bg: 'bg-indigo-100', border: 'border-l-indigo-500', text: 'text-indigo-700' },
  'Exotic Animal':         { bg: 'bg-emerald-100', border: 'border-l-emerald-600', text: 'text-emerald-700' },
}
// Fallback for custom types: bg-gray-100, border-l-gray-400, text-gray-700
```

### Navigation

- **Fleches prev/next** : changent de semaine. Label affiche : "Mar 10 - 16, 2026"
- **Bouton Today** : retourne a la semaine courante, scroll vers l'heure actuelle
- **Toggle Day/Week/Month** : segmented control (shadcn Tabs ou boutons). Week est actif par defaut. Day et Month sont desactives visuellement (tooltip "Coming soon") -- ils seront implementes dans la tache 002.

### Filtre par veterinaire

- Dropdown multi-select en haut a droite : "All vets" par defaut
- Quand un ou plusieurs vets sont selectionnes, seuls leurs RDV sont affiches
- Les RDV filtres disparaissent avec une animation fade-out

### Semaine UAE

- La semaine commence le **dimanche** (pas lundi)
- Les colonnes **vendredi et samedi** sont affichees en fond grise (hors jours ouvrables par defaut)
- Les heures hors-ouverture (avant 08:00, apres 18:00 par defaut) sont en fond legerement grise

### RTL support

- En mode arabe : les jours vont de droite a gauche (dimanche a droite)
- Les fleches de navigation sont inversees
- Les labels de jours sont traduits (next-intl)

### Responsive

- **Desktop (> 1024px)** : 7 colonnes visibles
- **Tablet (768-1024px)** : 3 jours visibles (dimanche-mardi par defaut), swipe horizontal pour voir les autres
- **Mobile (< 768px)** : redirige vers la vue jour (tache 002) -- pour l'instant, affiche 1 jour avec boutons prev/next

---

## MSW : enrichir les mock data

Ajouter `consultationType` et `durationMinutes` aux appointments mock :

```typescript
// Enrichir AppointmentDto
interface AppointmentDto {
  // ... champs existants
  consultationType: string    // "Vaccination", "Surgery", etc.
  durationMinutes: number     // 20, 30, 45, 60
}
```

Enrichir les 6 appointments mock existants avec des types varies et ajouter 4-5 appointments supplementaires pour avoir une semaine bien remplie (repartis sur la semaine courante).

---

## i18n

Toutes les chaines visibles doivent passer par next-intl :
- `calendar.today`, `calendar.prev`, `calendar.next`
- `calendar.views.day`, `calendar.views.week`, `calendar.views.month`
- `calendar.allVets`, `calendar.filterByVet`
- Noms des jours : via `Intl.DateTimeFormat` avec la locale courante

---

## data-testid obligatoires

- `calendar-week-view` -- conteneur principal
- `calendar-header` -- barre de navigation
- `calendar-prev-btn`, `calendar-next-btn`, `calendar-today-btn`
- `calendar-view-toggle` -- toggle Day/Week/Month
- `calendar-vet-filter` -- dropdown filtre vet
- `calendar-day-column-{dayIndex}` -- chaque colonne de jour (0=dimanche)
- `appointment-block-{appointmentId}` -- chaque bloc RDV
- `calendar-time-column` -- colonne des heures

---

## Critere de completion

```
[ ] Vue semaine s'affiche avec les 7 jours et les creneaux horaires
[ ] Les RDV apparaissent comme des blocs colores par type de consultation
[ ] Navigation prev/next/today fonctionne
[ ] Filtre par vet fonctionne
[ ] Semaine UAE : dimanche-jeudi en surbrillance, vendredi-samedi grise
[ ] RTL : jours de droite a gauche en arabe
[ ] Responsive : 7 cols desktop, 3 cols tablet, 1 col mobile
[ ] Clic sur un RDV navigue vers le detail
[ ] MSW handlers enrichis avec consultationType et durationMinutes
[ ] i18n : toutes les chaines traduites EN + AR
[ ] data-testid presents sur tous les elements interactifs
[ ] Tests Playwright passent
[ ] npm run build : 0 erreurs
[ ] Renommer en done-front-agenda-calendar-001.md
```
