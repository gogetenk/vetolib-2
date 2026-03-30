# todo-wire-breeding-027.md -- Wire breeding frontend to real API (fix URLs + contracts)

**Module** : Frontend (Breeding)
**Priority** : Haute
**Dependencies** : done-back-breeding-litter-008, done-front-breeding-012

## Context
QA found 2 wire-time blockers:
1. Frontend breeding API URLs missing /v1/ prefix (all calls will 404)
2. CreateLitterRequest missing bornCount/aliveCount fields

## Scope
1. Fix all API URLs in `src/frontend/src/lib/api/breeding.ts`:
   - /api/litters → /api/v1/litters
   - /api/pregnancies → /api/v1/breeding/pregnancies
   - /api/heat-cycles → /api/v1/patients/{id}/heat-cycles
   - /api/lineage → /api/v1/patients/{id}/lineage
2. Fix CreateLitterRequest to include bornCount, aliveCount, externalFatherName
3. Update RegisterLitterDialog to add born/alive count inputs
4. Update MSW handlers to match corrected URLs
