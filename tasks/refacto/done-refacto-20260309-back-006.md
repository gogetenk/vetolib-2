# todo-refacto-20260309-back-006 — PatientOwner.Create ne retourne pas Result<T>
**Priorite** : importante
**Fichiers concernes** :
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Domain/PatientOwner.cs` (ligne 15)
**Violation** : La factory method `PatientOwner.Create()` retourne directement un `PatientOwner` au lieu de `Result<PatientOwner>`. Selon CLAUDE.md regle 1 : "Chaque methode qui peut echouer retourne Result<T> ou Result". Meme si la validation est minimale, le pattern doit etre uniforme.
**Correction attendue** : Modifier la signature en `public static Result<PatientOwner> Create(Guid clinicId, Guid patientId, Guid ownerId)` avec validation des Guid.Empty et retour `Result<PatientOwner>.Success(...)`. Mettre a jour les appelants (CreatePatientHandler, etc.).
**Critere** : `PatientOwner.Create` retourne `Result<PatientOwner>` et les appelants gerent le resultat.
