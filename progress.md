# progress.md — Vetolib

_Mis à jour par l'orchestrator à chaque cycle._

## 2026-03-10 — Cycle forge (vague 4)

- TODO: 17 | WIP: 6 agents | DONE: 113
- Agents actifs : messaging notifications, messaging integrations (x3), prescriptions override, front agenda+billing+medical, prescriptions stock link (x2)
- PRs en review : 0
- Questions PO : 4 ouvertes
- Refacto : 3 nouvelles tâches (messaging DI critique, portal auth duplication, IgnoreQueryFilters doc)

## Audit archi — 2026-03-10 (Module Messaging)

- **Critique** : IMessageRouter + ITriageOrchestrator non enregistrés dans le DI → tâche refacto 008
- **Important** : Endpoint /portal/categories duplique la logique d'auth magic link → tâche refacto 009
- **Mineur** : IgnoreQueryFilters (15 occurrences) insuffisamment documenté → tâche refacto 010
- Conformité globale : ATTENTION (1 critique bloquant le runtime)

## Modules complétés (DONE: 113)

### Core
- Auth, Agenda, MedicalRecords, Billing, Notifications — matures
- AI scaffold (triage, no-show, interactions) — opérationnel
- Stock management — opérationnel

### Messaging (QUASI-COMPLET)
| Task | Statut |
|---|---|
| domain + hours + staff handlers + owner portal + templates + SSE | DONE |
| notifications + escalation | WIP (agent actif) |
| agenda + medical + AI integrations | WIP (agent actif) |
| front conversation detail + admin settings + owner portal + realtime | DONE |
| front playwright E2E | TODO |

### Prescriptions AI (EN COURS)
| Task | Statut |
|---|---|
| catalog + enrichment + weight + interactions | DONE |
| override + audit trail | WIP (agent actif) |
| stock link + stock suggest | WIP (agent actif) |
| front catalog + alerts | DONE |
| front stock + E2E | TODO |

### Infrastructure
| Task | Statut |
|---|---|
| Docker, HTTPS, monitoring, secrets, audit, rate limiting | DONE |
| CI pipeline (quality gates, security scan) | DONE |
| CD pipeline (staging + prod mock) | DONE |
| SemVer release-please | DONE |

### Tâches restantes (TODO: 17)
- agenda-002, medical-002
- back-api-public-001, back-multi-clinic-001
- front-agenda-001, front-billing-001, front-medical-001
- front-messaging-playwright-001, front-prescriptions-e2e-001, front-prescriptions-stock-001
- 3 refacto tâches (messaging DI, portal auth, IgnoreQueryFilters)
