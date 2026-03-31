# todo-p0-clinic-search-031.md — Clinic search / directory endpoint

**Module** : Auth (clinics are in Auth)
**Priority** : P0
**Dependencies** : aucune
**Skills** : `ardalis-result`, `aspnet-minimal-api`
**BDD** : `tests/Vetolib.Tests.Acceptance/Features/Portal/ClinicSearch.feature`

## Scope
1. Public endpoint: GET /api/v1/clinics/search?name=X&city=Y&species=Z
2. Returns: clinic name, city, supported species, logo, slug
3. No auth required (public directory)
4. Pagination
5. Unit tests

## Definition of Done
- [ ] Search endpoint with name/city/species filters
- [ ] Public (no auth)
- [ ] Paginated
- [ ] Unit tests
- [ ] BDD scenarios addressed
- [ ] Build GREEN
