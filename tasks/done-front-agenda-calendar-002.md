# todo-front-agenda-calendar-002.md -- Day view and month view

**Module** : Frontend / Agenda
**Dependances** : todo-front-agenda-calendar-001 (week view must be done first)
**Priorite** : HAUTE
[MSW: oui] -- developpement sans backend requis
**Skills a lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Objectif

Implementer les vues **jour** et **mois** du calendrier agenda, et activer le toggle Day/Week/Month qui etait en "Coming soon" dans la tache 001.

---

## Perimetre exact

### Fichiers a creer / modifier

- `components/features/calendar/DayCalendar.tsx` -- vue jour complete
- `components/features/calendar/MonthCalendar.tsx` -- vue mois grille
- `components/features/calendar/MonthDayCell.tsx` -- cellule individuelle dans la grille mois
- `components/features/calendar/CalendarHeader.tsx` -- activer le toggle Day/Week/Month (etait disabled)
- `app/[locale]/(dashboard)/appointments/page.tsx` -- ajouter le switch entre les 3 vues
- `e2e/calendar/calendar-day.spec.ts` -- tests Playwright vue jour
- `e2e/calendar/calendar-month.spec.ts` -- tests Playwright vue mois

---

## Spec UX : Vue Jour (DayCalendar)

### Layout

```
+------------------------------------------------------------------+
| < Prev  [Today]  Next >     [*Day*] [Week] [Month]   [Vet: All] |
+------------------------------------------------------------------+
| Thursday, March 12, 2026                                         |
+------------------------------------------------------------------+
| 07:00  |                                                         |
| 07:15  |                                                         |
| 07:30  |                                                         |
| 07:45  |                                                         |
| 08:00  | [=== Max - Vaccination - Dr. Sarah ==================] |
| 08:15  | [====================================================] |
| 08:30  |                                                         |
| ...                                                              |
+------------------------------------------------------------------+
```

### Specificites

- **Creneaux de 15 min** (plus fins que la vue semaine qui est en 30 min)
- **Colonne unique** occupant toute la largeur
- **Blocs RDV plus detailles** qu'en vue semaine :
  - Ligne 1 : Nom du patient + espece + nom du proprietaire
  - Ligne 2 : Type de consultation (badge) + veterinaire assigne
  - Ligne 3 : Raison/motif du RDV (text-xs, truncated)
  - Badge statut visible
- **Scroll automatique** vers l'heure actuelle (indicateur "now" = ligne rouge horizontale)
- **Navigation** : prev/next changent de jour
- **Label** : "Thursday, March 12, 2026" (format long, localise)
- **Chevauchements** : si 2 RDV sont au meme creneau, ils s'affichent cote a cote (50% de largeur chacun, 33% si 3, etc.)

### Responsive

- La vue jour est la **vue par defaut sur mobile** (< 768px)
- Swipe gauche/droite pour changer de jour (optionnel, boutons prev/next suffisent pour le MVP)

---

## Spec UX : Vue Mois (MonthCalendar)

### Layout

```
+------------------------------------------------------------------+
| < Prev  [Today]  Next >     [Day] [Week] [*Month*]  [Vet: All]  |
+------------------------------------------------------------------+
|  Sun  |  Mon  |  Tue  |  Wed  |  Thu  |  Fri  |  Sat  |
+-------+-------+-------+-------+-------+-------+-------+
|       |       |   1   |   2   |   3   |   4   |   5   |
|       |       | *3 RDV|       | *1 RDV|       |       |
+-------+-------+-------+-------+-------+-------+-------+
|   6   |   7   |   8   |   9   |  10   |  11   |  12   |
|       | *2 RDV|       | *5 RDV| *1 RDV|       | *4 RDV|
+-------+-------+-------+-------+-------+-------+-------+
| ...                                                     |
```

### Specificites

- **Grille 7 colonnes x 5-6 lignes** (selon le mois)
- Chaque cellule affiche :
  - Numero du jour
  - **Max 3 RDV visibles** : heure + nom patient + pastille couleur type
  - Si plus de 3 : lien "+N more" qui ouvre un popover avec la liste complete
- **Clic sur un jour** : bascule en vue jour pour ce jour
- **Clic sur un RDV dans la cellule** : navigue vers le detail
- **Jour courant** : fond legerement colore (blue-50)
- **Jours hors mois courant** : texte grise (text-muted-foreground)
- **Vendredi/samedi** : fond grise (jours non ouvrables UAE)
- **Navigation** : prev/next changent de mois
- **Label** : "March 2026"

### Indicateurs visuels par jour

- Petit cercle colore sous le numero du jour si des RDV existent (Google Calendar style)
- Nombre total de RDV entre parentheses si > 3

---

## Toggle Day/Week/Month

Le `CalendarHeader` de la tache 001 avait les boutons Day et Month desactives. Cette tache les active :

- Utiliser un **segmented control** (3 boutons groupes)
- L'etat de la vue est dans un state React (`'day' | 'week' | 'month'`)
- Au changement de vue, la date courante est conservee (si on est sur la semaine du 12 mars et on passe en jour, on voit le 12 mars)
- L'URL ne change pas (pas de routing par vue -- c'est un state local)

---

## RTL support

- Vue mois : les jours vont de droite a gauche
- Vue jour : le contenu du bloc est aligne a droite
- Labels de jours et mois : localises via `Intl.DateTimeFormat`

---

## i18n (chaines supplementaires)

- `calendar.dayLabel` : format date long ("Thursday, March 12, 2026")
- `calendar.monthLabel` : format mois ("March 2026")
- `calendar.moreAppointments` : "+{count} more"
- `calendar.noAppointments` : "No appointments"
- `calendar.comingSoon` : supprime (plus necessaire)

---

## data-testid obligatoires

- `calendar-day-view` -- conteneur vue jour
- `calendar-month-view` -- conteneur vue mois
- `calendar-now-indicator` -- ligne "maintenant" dans la vue jour
- `month-day-cell-{date}` -- chaque cellule du mois (format YYYY-MM-DD)
- `month-more-link-{date}` -- lien "+N more"
- `calendar-view-day-btn`, `calendar-view-week-btn`, `calendar-view-month-btn` -- boutons toggle

---

## Critere de completion

```
[ ] Vue jour affiche les RDV en creneaux de 15 min
[ ] Vue jour : indicateur "now" (ligne rouge) positionne correctement
[ ] Vue jour : chevauchements geres (blocs cote a cote)
[ ] Vue mois affiche la grille avec max 3 RDV par jour
[ ] Vue mois : "+N more" fonctionne (popover)
[ ] Vue mois : clic sur un jour bascule en vue jour
[ ] Toggle Day/Week/Month fonctionne et conserve la date courante
[ ] RTL : les 3 vues fonctionnent en arabe
[ ] Mobile : vue jour par defaut (< 768px)
[ ] i18n : toutes les nouvelles chaines traduites EN + AR
[ ] data-testid presents sur tous les elements
[ ] Tests Playwright passent (vue jour + vue mois)
[ ] npm run build : 0 erreurs
[ ] Renommer en done-front-agenda-calendar-002.md
```
