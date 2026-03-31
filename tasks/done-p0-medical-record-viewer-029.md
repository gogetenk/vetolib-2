# todo-p0-medical-record-viewer-029.md — Owner medical record viewer (portal endpoint)

**Module** : MedicalRecords + Portal
**Priority** : P0
**Dependencies** : todo-p0-owner-registration-backend-028
**Skills** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`
**BDD** : `tests/Vetolib.Tests.Acceptance/Features/Portal/MedicalRecordViewer.feature`

## Context
Owners need to view their animal's medical records. The data exists (GetPatientSummaryHandler). Need a portal-scoped endpoint with owner authorization.

## Scope
1. Add IOwnerAuthorizationService: verify owner is linked to the animal
2. Portal endpoint: GET /api/v1/portal/animals/{id}/medical-records
3. Returns: examinations, vaccinations, active prescriptions, weight history
4. Vet-controlled visibility: records flagged as "internal" are excluded
5. Add "isVisibleToOwner" flag on MedicalRecord entity (default true)

## Definition of Done
- [ ] IOwnerAuthorizationService created
- [ ] Portal endpoint returns medical data
- [ ] Internal notes excluded
- [ ] Visibility flag on MedicalRecord
- [ ] Unit tests
- [ ] BDD scenarios from MedicalRecordViewer.feature addressed
- [ ] Build + tests GREEN
