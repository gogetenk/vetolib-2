# todo-p0-owner-registration-backend-028.md — Owner registration backend (cross-clinic identity)

**Module** : Auth
**Priority** : P0 (avant lancement)
**Dependencies** : aucune
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`
**BDD** : `tests/Vetolib.Tests.Acceptance/Features/Portal/OwnerRegistration.feature`

## Context
Doctolib model: 1 owner account = access to all clinics. Currently owners are tenant-scoped (1 owner per clinic). Need a global owner identity.

## Scope
1. Create `OwnerAccount` entity in Auth module (global, NOT tenant-scoped): Email, Phone, PasswordHash, Name, IsVerified
2. Link existing Owner (MedicalRecords, tenant-scoped) to OwnerAccount via OwnerAccountId
3. Registration endpoint: POST /api/v1/portal/register (WhatsApp OTP primary, email fallback)
4. Login endpoint: POST /api/v1/portal/login (returns JWT with list of linked clinics)
5. Auto-link by microchip: when owner registers, search all clinics for animals with matching microchip
6. Fallback link by name+species+clinic

## Definition of Done
- [ ] OwnerAccount entity with Result<T> factory
- [ ] Registration + login endpoints
- [ ] Auto-link by microchip
- [ ] Unit tests for registration + linking logic
- [ ] Migration
- [ ] BDD scenarios from OwnerRegistration.feature addressed
- [ ] `dotnet build` + `dotnet test` GREEN
