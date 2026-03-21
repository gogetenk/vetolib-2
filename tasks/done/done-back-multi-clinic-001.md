# todo-back-multi-clinic-001.md — Multi-clinic switcher

**Module** : Auth
**Dependencies** : done-back-signup-selfservice-001
**Priority** : low (post-MVP)
**Skills** : ardalis-result, multitenant-efcore

## Objective
Allow an admin to manage multiple clinics from a single account with a clinic switcher. NO consolidated dashboard.

## Scope

### Backend
1. **ClinicGroup entity** : `{ Id, Name, OwnerUserId }`
2. **ClinicGroupMember** : `{ ClinicGroupId, ClinicId }`
3. **Endpoints** :
   - `POST /api/v1/clinic-groups` — create group (AdminOnly)
   - `POST /api/v1/clinic-groups/{id}/clinics` — add clinic to group
   - `GET /api/v1/clinic-groups/{id}/clinics` — list clinics in group
4. **Switch clinic** : refresh JWT with new ClinicId

### Frontend
5. **Clinic switcher in header** (only visible if user has multiple clinics)
6. Switching refreshes the auth token and reloads data

## NOT in scope
- Consolidated cross-clinic dashboard
- Cross-clinic analytics

## Completion criteria
- [ ] ClinicGroup entity + endpoints
- [ ] Clinic switcher in header
- [ ] JWT refresh on switch
- [ ] Tests
- [ ] Build GREEN
