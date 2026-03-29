# todo-front-patient-extended-005.md -- Frontend: display Sex, Microchip, expanded Species

**Module** : Frontend (MedicalRecords)
**Priority** : Haute
**Dependencies** : todo-back-patient-sex-001, todo-back-patient-microchip-002, todo-back-patient-species-003
**Skills** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**
**[Branchement ulterier: wire-patient-extended]**

## Context

Backend tasks 001-003 add Sex, MicrochipNumber, and Falcon/Reptile species. The frontend needs to display these in patient cards, patient lists, and creation/edit forms. Develop with MSW first -- no backend dependency.

## Scope

### 1. MSW handlers

Update existing patient MSW handlers to include `sex`, `microchipNumber` in mock data.

### 2. Patient creation form

- Add Sex dropdown: Male, Female, Unknown (NeuteredMale/SpayedFemale only on edit)
- Add Microchip Number input field with mask (15 digits)
- Update Species dropdown to include Falcon and Reptile

### 3. Patient edit form

- Add Sex dropdown with all 5 values
- Add Microchip Number input field
- Display validation error for invalid microchip format

### 4. Patient card / detail view

- Display Sex as icon + text next to species/breed
- Display Microchip Number with chip icon (if present)
- Add Falcon and Reptile icons

### 5. Patient list

- Add Sex column (optional, sortable/filterable)
- Add "Search by microchip" option in search bar

### 6. data-testid

All interactive elements MUST have `data-testid`:
- `data-testid="patient-sex-select"`
- `data-testid="patient-microchip-input"`
- `data-testid="patient-species-select"`
- `data-testid="patient-sex-display"`
- `data-testid="patient-microchip-display"`

## BDD

No .feature file for frontend-only (Playwright tests will be added during wire task).

## Completion criteria

- [ ] Sex dropdown in create/edit forms
- [ ] Microchip input with 15-digit validation
- [ ] Falcon + Reptile in species dropdown
- [ ] Patient card shows Sex and Microchip
- [ ] MSW handlers return mock data with new fields
- [ ] All interactive elements have `data-testid`
- [ ] `npm run lint` + `npm run build` GREEN
