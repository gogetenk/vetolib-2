# todo-front-messaging-admin-settings-001.md — Pages settings admin (templates, heures, stats)

**Module** : Frontend (Messaging)
**Dependances** : todo-front-messaging-msw-001
**Priorite** : MOYENNE
**Skills a lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterieur]** : wire-messaging-admin

---

## Objectif

Creer les pages d'administration de la messagerie : gestion des templates, configuration des heures de bureau, et dashboard de statistiques.

## Spec de reference

`docs/MESSAGING-SPEC.md` section 4.2 (Templates management, Messaging hours config, Triage statistics)

## Implementation

### 1. Page templates

Route : `/[locale]/settings/messaging/templates`

- Liste des templates existants (tableau : Name, Category, Actions)
- Bouton "Add template" (data-testid="add-template-btn")
- Dialog creation/edition avec :
  - Name (input text)
  - Category (select optionnel)
  - Content EN (textarea)
  - Content AR (textarea, RTL)
  - Boutons Save / Cancel
- Bouton supprimer avec confirmation dialog

### 2. Page heures de messagerie

Route : `/[locale]/settings/messaging/hours`

- Tableau 7 jours de la semaine
- Pour chaque jour : toggle "Open/Closed" + heure ouverture + heure fermeture
- Pre-rempli avec le default UAE (Sunday-Thursday 08:00-20:00, Friday 08:00-12:00, Saturday closed)
- Bouton "Save" (data-testid="save-hours-btn")
- Note : "Messages sent outside these hours will receive an automatic acknowledgment"

### 3. Page statistiques de triage

Route : `/[locale]/settings/messaging/stats`

Dashboard avec :
- **Average first response time** : card avec valeur (ex: "2h 15min")
- **Messages by category** : pie chart (ou bar chart)
- **AI triage accuracy** : pourcentage de messages recategorises par le staff
- **Volume per day** : line chart sur 30 jours
- **Conversion rate** : message -> appointment (pourcentage)

Utiliser des composants shadcn/ui pour les cards. Pour les charts, utiliser recharts ou un composant simple.

### 4. Navigation

- Ajouter un sous-menu dans Settings : "Messaging" avec 3 sous-pages
- Accessible uniquement par Admin (masque pour les autres roles)

### 5. Composants

- `TemplatesPage.tsx`
- `TemplateFormDialog.tsx`
- `MessagingHoursPage.tsx`
- `DayHoursRow.tsx`
- `TriageStatsPage.tsx`
- `StatCard.tsx`

## Critere

```
[] Page templates avec CRUD complet
[] Page heures de messagerie avec default UAE
[] Page stats avec 5 metriques
[] Navigation settings/messaging ajoutee
[] Acces AdminOnly
[] data-testid sur tous les elements interactifs
[] Responsive
[] npm run dev fonctionne
[] Renommer en done
```
