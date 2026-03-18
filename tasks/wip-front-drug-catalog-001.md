# todo-front-drug-catalog-001.md — Page catalogue médicaments

**Module** : Frontend (MedicalRecords)
**Dépendances** : aucune
**Skills à lire** : `shadcn-nextjs`, `msw-mock-api`
**[MSW: oui]**

---

## Objectif

Créer une page `/[locale]/(dashboard)/drugs/page.tsx` pour consulter et gérer le catalogue de médicaments vétérinaires. Actuellement la recherche de médicaments n'existe que dans le formulaire de prescription — il faut une page dédiée.

## Backend existant (endpoints déjà implémentés)

```
GET    /api/v1/medical-records/drugs?search={term}  — Recherche catalogue
GET    /api/v1/medical-records/drugs/{id}            — Détail médicament
POST   /api/v1/medical-records/drugs                 — Ajout custom drug [VetOrAdmin]
```

## API client existant

`src/frontend/src/lib/api/drugs.ts` — `searchDrugs(term)`, `getDrugById(id)` déjà implémentés.

## MSW handlers existants

`src/frontend/src/mocks/handlers/drugs.ts` — 15 médicaments mockés avec interactions, contraindications, dosages.

## Implémentation

### 1. Page `/drugs`
- Barre de recherche avec debounce (300ms)
- Table des résultats : DisplayName, INN Name, Category, Species contraindications count, Active/Inactive
- Clic sur une ligne → panneau détail (drawer ou page détail)

### 2. Vue détail médicament
- Infos générales : INN, Display name, Category, RequiresPrescription
- Section "Interactions" : liste des DrugInteraction avec severity badges (Critical=red, Moderate=orange, Info=blue)
- Section "Contraindications" : liste par espèce avec severity et alternative drug
- Section "Dosage Guidelines" : table par espèce (min/max dose per kg, route)

### 3. Formulaire ajout custom drug
- Dialog/drawer pour ajouter un médicament custom à la clinique
- Champs : INN Name, Display Name, Category (select)
- Appel POST /api/v1/medical-records/drugs

### 4. Navigation
- Ajouter lien "Drug Catalog" dans la sidebar (icône Pill de lucide-react)
- Visible pour VET et ADMIN uniquement

### 5. i18n
- Ajouter les clés dans messages/en.json et messages/ar.json sous `drugs.*`

## Critère de complétion

```
□ Page /drugs accessible depuis la sidebar
□ Recherche avec résultats dans table
□ Vue détail avec interactions, contraindications, dosages
□ Formulaire ajout custom drug
□ data-testid sur tous éléments interactifs
□ i18n EN + AR
□ npm run lint → 0 errors
□ npm run build → 0 errors
□ PR vers develop
□ Renommer en done-front-drug-catalog-001.md
```
