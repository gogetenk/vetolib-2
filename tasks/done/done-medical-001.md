# tasks/done-medical-001.md
**Module** : MedicalRecords
**Status** : [DONE]
**Dépendances** : todo-auth-001.md
**Gherkins** : features/billing-and-records.feature (section medical-records)

## Description
Implémenter la gestion des animaux et propriétaires.

Endpoints : `POST /api/v1/patients`, `GET /api/v1/patients`, `GET /api/v1/patients/{id}`, `POST /api/v1/owners`

Inclure : lien animal-propriétaire (many-to-many), isolation tenant.

## Complétion
- [x] Entités Patient, Owner, PatientOwner avec IMultiTenant
- [x] Many-to-many via PatientOwner join entity
- [x] CQRS: CreatePatient, CreateOwner commands + ListPatients, GetPatientById queries
- [x] Minimal APIs avec ToMinimalApiResult()
- [x] FluentValidation + ValidationBehavior
- [x] EF Core configurations (schema "medical")
- [x] Tenant isolation via MultiTenantDbContext global query filter
- [x] 3/3 Reqnroll scenarios GREEN
