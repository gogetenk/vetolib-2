# todo-front-breeding-012.md -- Frontend: breeding dashboard (litters, lineage, pregnancy, heat)

**Module** : Frontend (Breeding)
**Priority** : Moyenne
**Dependencies** : todo-back-breeding-litter-008, todo-back-breeding-lineage-009, todo-back-breeding-pregnancy-010, todo-back-breeding-heatcycle-011
**Skills** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterier: wire-breeding]**

## Context

The Breeding module backend exposes endpoints for litters, lineage/pedigree, pregnancy tracking, and heat cycles. The frontend needs a comprehensive breeding section accessible from the patient detail page and a clinic-wide breeding dashboard.

## Scope

### 1. MSW handlers

Create handlers for all breeding endpoints:
- Litters: CRUD + offspring
- Lineage: set/get + pedigree tree
- Pregnancy: create, delivery, loss, checks
- Heat cycles: record, list, prediction

Use realistic UAE data (Arabian horses, Salukis, Falcons).

### 2. Patient detail — "Breeding" tab

Add a "Breeding" tab on the patient detail page (only visible for eligible patients -- Female or intact animals).

Tab content varies by sex:
- **Female patients**: Show litters, pregnancies, heat cycles
- **Male patients**: Show litters where they are the father, lineage

### 3. Litter management view

- List of litters for a mother (reverse chronological)
- Litter detail: mother, father, birth date, born/alive counts, offspring list
- "Register Litter" form (dialog):
  - Mother (pre-filled if from patient page)
  - Father (patient search or "External father" text input)
  - Birth date, born count, alive count, notes
- "Add Offspring" form on litter detail:
  - Name, sex, breed, microchip (optional)
  - Creates patient + links to litter

### 4. Pedigree tree view

- Visual tree component (up to 3 generations)
- Each node shows: name, species, breed, sex, registry number
- Clickable nodes navigate to patient detail
- "Set Parents" form:
  - Mother search (filtered to Female patients, same species)
  - Father search (filtered to Male patients, same species)
  - Registry number + type (LOF, LOOF, SIRE, EAHS, FEI, Other)

### 5. Pregnancy timeline view

- List of pregnancies for a patient
- Pregnancy detail: timeline (mating -> checks -> expected delivery)
  - Visual timeline component
  - Completed checks shown differently from scheduled
- "Record Pregnancy" dialog
- "Record Delivery" dialog
- "Record Loss" dialog
- "Schedule Check" dialog
- Clinic-wide "Active Pregnancies" dashboard page

### 6. Heat cycle view

- Table of heat cycles (start, end, duration, notes)
- Prediction card: "Next heat expected around {date}" with average interval
- "Record Heat Cycle" dialog (start date, end date, notes)
- Only visible to VET/ADMIN (not ASSISTANT)

### 7. Breeding dashboard (clinic-wide)

New page: `/dashboard/breeding`
- Active pregnancies (sorted by due date)
- Recent litters
- Upcoming predicted heat cycles

### 8. data-testid (all interactive elements)

- `data-testid="breeding-tab"`
- `data-testid="litter-list"`, `data-testid="litter-register-button"`
- `data-testid="pedigree-tree"`, `data-testid="pedigree-set-parents-button"`
- `data-testid="pregnancy-list"`, `data-testid="pregnancy-record-button"`
- `data-testid="pregnancy-timeline"`
- `data-testid="heat-cycle-list"`, `data-testid="heat-record-button"`
- `data-testid="heat-prediction-card"`
- `data-testid="breeding-dashboard"`

## BDD

No .feature for frontend-only. Playwright tests during wire task.

## Completion criteria

- [ ] Breeding tab on patient detail page
- [ ] Litter management (list, detail, register, add offspring)
- [ ] Pedigree tree visualization (3 generations)
- [ ] Pregnancy timeline view with all actions
- [ ] Heat cycle table with prediction
- [ ] Clinic-wide breeding dashboard
- [ ] MSW handlers for all breeding endpoints
- [ ] All interactive elements have `data-testid`
- [ ] Role-based visibility (heat cycles VET/ADMIN only)
- [ ] `npm run lint` + `npm run build` GREEN
