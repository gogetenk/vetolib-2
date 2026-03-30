# QA Nightly Review — PRs #157-#181 — 2026-03-30

**Reviewer**: QA Agent
**Overall status**: [QA_FAIL] — 4 blockers, 2 real bugs + 2 wire-time issues

## Results

| Section | Status |
|---|---|
| Backend build | PASS (0 errors) |
| Unit tests | PASS (1157/1157) |
| Frontend lint+build | PASS |
| Breeding module patterns | PASS with 1 blocker |
| Security fixes | PASS |
| Perf fixes | PASS |
| data-testid coverage | PASS |
| i18n | FAIL (ar.json gaps) |
| BDD compliance | PASS |
| Frontend/Backend contract | FAIL (2 wire-time issues) |

## Blockers

1. **PregnancyEndpoints.cs** hardcodes "Female"/"Dog" instead of reading patient data → wrong gestation for non-dog species
2. **ar.json** `medical_record_form` 22 keys untranslated (English in Arabic UI)
3. **Frontend CreateLitterRequest** missing bornCount/aliveCount (wire-time, MSW masks it)
4. **Frontend breeding API URLs** missing /v1/ prefix (wire-time, MSW masks it)

Blockers 1+2 = real bugs → fix dispatched.
Blockers 3+4 = wire-time issues → will be fixed during wire task.
