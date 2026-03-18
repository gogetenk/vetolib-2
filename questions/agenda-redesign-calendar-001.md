# Question — agenda-redesign-calendar-001

**Module** : Agenda
**Bloquant** : Non (feature request, pas un bug)
**Demandé par** : Product Owner (2026-03-12)

## Problème

L'UI actuelle de l'agenda est une **liste de rendez-vous** — c'est insuffisant pour un usage vétérinaire professionnel. Il faut une **vue calendrier** esthétique et fonctionnelle.

## Besoins exprimés

1. **Vue calendrier** (jour / semaine / mois) — pas une simple liste
2. **Couleurs par type de consultation** (vaccination, chirurgie, urgence, contrôle, etc.)
3. **UX professionnelle** — l'agenda est l'écran principal du vétérinaire, il doit être beau et efficace
4. **Drag & drop** pour déplacer des rendez-vous (nice to have)
5. **Vue condensée** des infos clés : patient, propriétaire, type, durée

## Actions attendues

1. **Designer UX** : créer des maquettes / wireframes de la vue calendrier
2. **PO** : valider les maquettes, définir les types de consultation avec couleurs associées
3. **PO** : créer les tâches `todo-front-agenda-redesign-*.md` pour les devs front

## Recommandation technique (pour info)

Librairies calendrier compatibles React/Next.js :
- `@schedule-x/react` (moderne, léger)
- `react-big-calendar` (mature, flexible)
- `@fullcalendar/react` (complet, premium pour certaines features)

Le choix final revient au designer/PO après maquettes.

---

## Reponse PO

### 1. Types de consultation et couleurs

Le backend `ConsultationType` est configurable par clinique (name, durationMinutes). Les couleurs doivent etre associees cote frontend a chaque type. Voici les **types par defaut** (pre-seeds) avec leurs couleurs :

| Type | Couleur (Tailwind) | Hex approx. | Duree par defaut |
|---|---|---|---|
| General Checkup | `blue-500` | #3B82F6 | 30 min |
| Vaccination | `green-500` | #22C55E | 20 min |
| Surgery | `red-500` | #EF4444 | 60 min |
| Emergency | `orange-500` | #F97316 | 30 min |
| Dental | `purple-500` | #A855F7 | 45 min |
| Dermatology | `pink-500` | #EC4899 | 30 min |
| Follow-up | `teal-500` | #14B8A6 | 20 min |
| Grooming | `amber-500` | #F59E0B | 30 min |
| Laboratory / Diagnostics | `indigo-500` | #6366F1 | 30 min |
| Exotic Animal | `emerald-600` | #059669 | 45 min |

**Regle** : la couleur est stockee dans le `ConsultationTypeDto` (champ `color` a ajouter cote backend -- tache back separee). En attendant, le frontend mappe les noms connus a des couleurs par defaut, avec un fallback `gray-400` pour les types custom.

### 2. Vues requises

| Vue | Description | Defaut |
|---|---|---|
| **Week** | 7 colonnes (dimanche-samedi), creneaux de 30 min, 07:00-21:00 | **OUI -- vue par defaut** |
| **Day** | 1 colonne, creneaux de 15 min, scroll vertical, vision detaillee | Non |
| **Month** | Grille mensuelle, max 3-4 RDV visibles par jour, +N pour le reste | Non |

**Semaine UAE** : la vue semaine affiche **dimanche a jeudi** en surbrillance (jours ouvrables par defaut), samedi et vendredi en grise. C'est configurable par clinique -- certaines cliniques travaillent aussi le samedi.

**Heures Ramadan** : les horaires d'ouverture de la clinique sont configures dans les preferences. Le calendrier affiche visuellement les heures hors-ouverture en grise (zone hachuree ou opacite reduite).

### 3. Informations affichees par creneau

Chaque bloc RDV dans le calendrier affiche :

**Vue semaine/jour (bloc complet)** :
- **Ligne 1** : Nom du patient + icone espece (emoji ou icone : Dog, Cat, Bird, etc.)
- **Ligne 2** : Nom du proprietaire
- **Ligne 3** : Type de consultation (badge colore)
- **Indicateur** : Duree (hauteur du bloc proportionnelle)
- **Badge statut** : petit indicateur colore (SCHEDULED=bleu, CHECKED_IN=jaune, IN_PROGRESS=vert, COMPLETED=gris, CANCELLED=rouge barre)

**Vue mois (bloc condense)** :
- Heure + Nom du patient + pastille couleur du type
- Tooltip au hover avec le detail complet

### 4. Interactions cles

| Interaction | Comportement |
|---|---|
| **Clic sur creneau vide** | Ouvre le formulaire de creation rapide (pre-rempli avec date/heure du creneau) |
| **Clic sur un RDV** | Ouvre un panel lateral (sheet/drawer) avec le detail complet + actions de transition |
| **Drag & drop** | Deplace un RDV a un autre creneau (uniquement si statut = SCHEDULED). Confirmation dialog avant validation. **MVP : nice-to-have, tache separee si le temps le permet.** |
| **Boutons navigation** | Fleches gauche/droite pour semaine precedente/suivante + bouton "Today" |
| **Toggle vues** | Boutons segmentes : Day / Week / Month |
| **Filtre par vet** | Dropdown pour filtrer par veterinaire (multi-select) -- en header du calendrier |

### 5. Comportement mobile / responsive

- **Mobile (< 768px)** : vue jour uniquement (pas de semaine/mois -- trop etroit). Swipe gauche/droite pour changer de jour.
- **Tablet (768-1024px)** : vue jour ou semaine (3 jours visibles). Pas de mois.
- **Desktop (> 1024px)** : toutes les vues disponibles.

Le drag & drop est desactive sur mobile (tactile = imprecis). A la place, un bouton "Reschedule" dans le detail du RDV.

### 6. RTL (Arabic)

- Le calendrier se lit de droite a gauche en mode AR
- Les jours de la semaine sont affiches en arabe
- Les fleches de navigation sont inversees (fleche droite = semaine precedente en RTL)
- Les heures restent en format 12h (AM/PM) qui est le standard UAE

### 7. Taches creees

- `todo-front-agenda-calendar-001.md` -- Composant calendrier principal (vue semaine)
- `todo-front-agenda-calendar-002.md` -- Vue jour et vue mois
- `todo-front-agenda-calendar-003.md` -- Creation rapide de RDV depuis le calendrier

-> Debloque : aucune tache existante bloquee
-> Escalade humain requise : non
