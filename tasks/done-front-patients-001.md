# todo-front-patients-001.md — Frontend : Patients standalone

**Module** : Frontend / Patients
**Dépendances** : done-front-layout-001
[MSW: oui] — développement sans backend requis
[Branchement ultérieur] : done-back-patients-001 → crée automatiquement todo-wire-patients-001
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`, `veterinary-domain`

---

## Périmètre exact

- `app/(dashboard)/patients/page.tsx` — liste avec recherche
- `app/(dashboard)/patients/new/page.tsx` — création standalone
- `app/(dashboard)/patients/[id]/page.tsx` — dossier complet (amélioration de l'existant)
- `components/features/patients/PatientForm.tsx`
- `components/features/patients/SpeciesIcon.tsx`
- `lib/api/patients.ts` (étendre l'existant)
- `src/mocks/handlers/patients.ts` (étendre l'existant)
- `e2e/patients/patients-standalone.spec.ts`

## UI liste patients

Barre de recherche en haut (recherche par nom ou propriétaire, live)
Grid de cards ou tableau selon la préférence de l'agent :

Chaque entrée : icône espèce + nom + race + âge calculé + propriétaire + badge dernier statut RDV

Bouton "Add Patient" (haut droit) — VET et ADMIN uniquement

## Formulaire création patient

```
Nom de l'animal *
Espèce * (Select avec icônes : 🐕 Dog | 🐈 Cat | 🐦 Bird | 🐇 Rabbit |
                                🐴 Horse | 🐪 Camel | ⭐ Exotic)
Race (text libre)
Date de naissance * (DatePicker — pas de date future)
Sexe (Select : Male | Female | Unknown)

--- Propriétaire ---
Nom complet *
Téléphone * (format UAE : +971 XX XXX XXXX)
Email (optionnel)

[Cancel]  [Save Patient]
```

## Dossier patient — améliorer l'existant

La page `patients/[id]/page.tsx` existe déjà (done-front-medical-001).
Ajouter uniquement :
- Bouton "Edit" sur le header (ouvre un drawer avec le formulaire pré-rempli)
- Affichage de l'âge calculé (ex: "3 years 2 months")
- Icône espèce dans le header

## MSW handlers à étendre

```typescript
// Ajouter dans src/mocks/handlers/patients.ts
// POST /api/patients
// PATCH /api/patients/:id
// Données UAE : Camel comme espèce disponible
const MOCK_PATIENTS = [
  { id: 'p-001', name: 'Max', species: 'Dog', breed: 'Golden Retriever',
    birthDate: '2020-03-15', ownerName: 'Ahmed Al-Mansoori',
    ownerPhone: '+971 50 123 4567', clinicId: 'clinic-001' },
  { id: 'p-002', name: 'Layla', species: 'Camel', breed: 'Dromedary',
    birthDate: '2018-07-20', ownerName: 'Khalid Al-Mazrouei',
    ownerPhone: '+971 55 987 6543', clinicId: 'clinic-001' },
]
```

## RBAC côté UI

- RECEPTIONIST et ASSISTANT : pas de bouton "Add Patient", pas de bouton "Edit"
- Lecture seule pour ces rôles

## Critère de complétion

```
□ Liste patients avec recherche live
□ Création patient fonctionne (espèce Camel disponible)
□ Format téléphone UAE dans le formulaire
□ Dossier patient affiche l'âge calculé + bouton Edit
□ RECEPTIONIST/ASSISTANT : lecture seule
□ Tests Playwright verts
□ Renommer en done-front-patients-001.md
```
