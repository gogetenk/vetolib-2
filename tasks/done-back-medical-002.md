# tasks/todo-medical-002.md
**Module** : MedicalRecords  
**Status** : [TODO]  
**Dépendances** : todo-medical-001.md  
**Gherkins** : features/billing-and-records.feature (examens + ordonnances)

## Description
Implémenter les examens médicaux et ordonnances.

Endpoints : `POST /api/v1/patients/{id}/records`, `GET /api/v1/patients/{id}/records`, `POST /api/v1/patients/{id}/records/{rid}/prescriptions`

Inclure : soft delete uniquement, numéro de licence vétérinaire sur ordonnances, permissions VET only pour l'écriture.
