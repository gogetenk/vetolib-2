# QA Night Operation — 2026-04-01

## Status: PHASE 2+3 (Master plan + QA vague 1 dispatched — 10 agents parallel)

## Phases
- [ ] Phase 1: Master plan exhaustif (agent en cours)
- [ ] Phase 2: Découpage en micro-missions
- [ ] Phase 3: Dispatch massif (~20 agents)
- [ ] Phase 4: Consolidation + rapport final

## Micro-missions (à remplir après Phase 1)

### Frontend — Screenshots + UI Review
| # | Périmètre | Agent ID | Status | Rapport |
|---|---|---|---|---|
| F1 | Dashboard + stats | — | TODO | — |
| F2 | Calendar (day/week/month) | — | TODO | — |
| F3 | Patient list + detail + forms | — | TODO | — |
| F4 | Appointments (list, detail, create) | — | TODO | — |
| F5 | Billing (invoices, create, PDF) | — | TODO | — |
| F6 | Messaging (conversations, templates) | — | TODO | — |
| F7 | Breeding (litter, pregnancy, heat, pedigree) | — | TODO | — |
| F8 | Settings (team, working hours, reminders, audit) | — | TODO | — |
| F9 | Portal owner (pets, records, sharing, booking) | — | TODO | — |
| F10 | Landing pages (B2B, B2C, pricing, developers) | — | TODO | — |

### Backend — Endpoint contract testing
| # | Module | Agent ID | Status | Rapport |
|---|---|---|---|---|
| B1 | Auth (login, register, users, clinics, groups) | — | TODO | — |
| B2 | Agenda (appointments, schedule, waitlist, feedback) | — | TODO | — |
| B3 | MedicalRecords (patients, records, prescriptions, drugs) | — | TODO | — |
| B4 | Billing (invoices, PDF, CSV, e-invoicing) | — | TODO | — |
| B5 | Breeding (litter, pregnancy, heat, lineage) | — | TODO | — |
| B6 | Messaging + Notifications + Stock + AI | — | TODO | — |

### Quality — Performance + Security
| # | Périmètre | Agent ID | Status | Rapport |
|---|---|---|---|---|
| Q1 | Slow query audit (N+1, missing indexes) | — | TODO | — |
| Q2 | Security pen test (injection, XSS, IDOR, auth bypass) | — | TODO | — |
| Q3 | Dead code + dead wiring rescan | — | TODO | — |
| Q4 | Migration consistency (all contexts clean) | — | TODO | — |

## Rapport final
- [ ] Consolidation agent dispatché
- [ ] Rapport écrit dans docs/audits/qa-night-final-report-20260401.md
