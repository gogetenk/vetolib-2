# todo-front-medical-001.md — Frontend : Dossiers Médicaux

**Module** : Frontend / Medical Records
**Dépendances** : front-scaffold-000, back-medical-001
[MSW: oui] — développement sans backend requis
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`, `veterinary-domain`

---

## Périmètre exact

- `app/(dashboard)/patients/page.tsx` — liste des animaux
- `app/(dashboard)/patients/[id]/page.tsx` — dossier complet de l'animal
- `app/(dashboard)/patients/[id]/records/new/page.tsx` — nouveau compte-rendu
- `components/features/patients/PatientCard.tsx`
- `components/features/patients/MedicalRecordsList.tsx`
- `components/features/patients/MedicalRecordForm.tsx`
- `lib/api/patients.ts` + `lib/api/medical-records.ts`
- `e2e/patients/medical-records.spec.ts`

---

## Liste des animaux (patients)

Tableau ou cards grid avec :
- Nom, Espèce (icône), Race, Âge, Propriétaire
- Dernier RDV, Prochain RDV
- Bouton "View Record"
- Recherche par nom ou propriétaire
- `data-testid` : "patients-table", "search-input"

---

## Dossier animal — page détail

**Header** :
- Photo (placeholder avatar par espèce), Nom, Espèce, Race, Âge
- Propriétaire + téléphone
- Bouton "New Medical Record"

**Tabs shadcn** :
1. **Medical Records** — liste des comptes-rendus (date, vétérinaire, motif, résumé)
2. **Prescriptions** — ordonnances actives + historique
3. **Vaccinations** — tableau vaccins + dates rappel

---

## Formulaire nouveau compte-rendu

Champs :
- Motif de consultation (text)
- Anamnèse (Textarea)
- Examen clinique : Poids (number, kg), Température (number, °C), Fréquence cardiaque (number)
- Diagnostic (Textarea)
- Traitement prescrit (Textarea)
- Ordonnance (Textarea, optionnel)
- Prochain RDV recommandé (DatePicker, optionnel)

RBAC : seuls VET peuvent créer un compte-rendu. ASSISTANT voit en lecture seule.

---

## Critère de complétion

```
□ Liste des patients accessible
□ Dossier animal complet avec tous les onglets
□ Création compte-rendu fonctionne (VET seulement)
□ ASSISTANT ne voit pas le bouton "New Medical Record"
□ Tests Playwright passent
□ Renommer en done-front-medical-001.md
```
