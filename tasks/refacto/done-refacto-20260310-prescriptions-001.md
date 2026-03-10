# todo-refacto-20260310-prescriptions-001 -- French validation messages in Patient.Create

**Priorite** : mineure
**Fichiers concernes** :
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/Patient.cs`

**Violation** : UAE market rule -- all user-facing strings must be in English (see CLAUDE.md, Memory: "Language: EN (not FR) -- UAE market").

**Details** :
- `Patient.Create()` line 33: `"Le nom de l'animal est requis"` -> `"Patient name is required"`
- `Patient.Create()` line 36: `"La race est requise"` -> `"Breed is required"`
- `Patient.Create()` line 39: `"La date de naissance est requise"` -> `"Birth date is required"`
- `Patient.UpdateInfo()` line 64: `"Le nom de l'animal ne peut pas etre vide"` -> `"Patient name cannot be empty"`
- `Patient.UpdateInfo()` line 73: `"La race ne peut pas etre vide"` -> `"Breed cannot be empty"`
- `Patient.UpdateInfo()` line 80: `"La date de naissance est invalide"` -> `"Birth date is invalid"`

**Correction attendue** : Replace all French validation messages with English equivalents.
**Critere** : [] grep -r "est requis\|ne peut pas" Patient.cs returns no results
