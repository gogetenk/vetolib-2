# tasks/todo-medical-001.md
**Module** : MedicalRecords  
**Status** : [TODO]  
**Dépendances** : todo-auth-001.md  
**Gherkins** : features/billing-and-records.feature (section medical-records)

## Description
Implémenter la gestion des animaux et propriétaires.

Endpoints : `POST /api/v1/patients`, `GET /api/v1/patients`, `GET /api/v1/patients/{id}`, `POST /api/v1/owners`

Inclure : lien animal-propriétaire (many-to-many), isolation tenant.
