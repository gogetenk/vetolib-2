# todo-refacto-20260309-front-003 -- Aligner DTOs frontend avec le contrat API
**Priorite** : importante
**Fichiers concernes** :
- `src/frontend/src/lib/api/appointments.ts`
- `src/frontend/src/lib/api/patients.ts`
- `src/frontend/src/lib/api/medical-records.ts`
- `src/frontend/src/lib/api/billing.ts`
- `src/frontend/src/mocks/handlers/*.ts`
- `docs/api-contract.md`

**Violation** : Divergences entre les DTOs frontend et le contrat API qui casseront au wire

**Correction attendue** :

Choisir un des deux chemins :
- **Option A** : Mettre a jour `docs/api-contract.md` pour refleter les DTOs enrichis du frontend (recommande si le backend n'est pas encore fige)
- **Option B** : Aligner le frontend sur le contrat existant

Points de divergence a resoudre :
1. **Status casing** : Frontend `SCHEDULED` vs Contrat `Scheduled` -- decider UPPER_CASE ou PascalCase
2. **PatientDto** : `dateOfBirth` vs `birthDate`, champs supplementaires (`ageYears`, `gender`, `ownerEmail`, `lastVisitDate`, `nextAppointmentDate`)
3. **MedicalRecordDto** : Frontend a `reason, anamnesis, weight, temperature, heartRate` non decrits dans le contrat ; `prescription: string` vs `prescriptions: PrescriptionDto[]`
4. **InvoiceDto.vatRate** : `5` (pourcentage entier) vs `0.05` (fraction decimale)
5. **Auth.expiresIn** : `3600` vs `900`
6. **InvoiceForm MOCK_PATIENTS** : Remplacer par un appel API `GET /api/patients`

**Critere** :
- [ ] Le contrat API et les DTOs frontend utilisent le meme casing pour les statuts
- [ ] Les noms de champs sont alignes (birthDate vs dateOfBirth)
- [ ] InvoiceForm charge les patients depuis l'API, pas de MOCK_PATIENTS hardcode
- [ ] Le fichier `docs/api-contract.md` est a jour
