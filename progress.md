# progress.md — Vetolib

_Mis à jour par l'orchestrator à chaque cycle._

## 2026-03-10 — Cycle orchestrateur (reprise après déconnexion)

- TODO: 22 | WIP: 8 | DONE: 99
- Agents actifs : 8 dev agents dispatched
- PRs en review : 0
- Questions PO : 2 (whatsapp-integration-001, drug-catalog-placement-001)
- Prochaine action : attendre résultats des 8 agents, puis dispatcher vague 3

## Agents en cours

| Agent | Tâche | Module | Type |
|---|---|---|---|
| dev-1 | back-messaging-staff-handlers-001 | Messaging | Backend |
| dev-2 | back-messaging-owner-portal-001 | Messaging | Backend |
| dev-3 | back-messaging-templates-001 | Messaging | Backend |
| dev-4 | back-prescriptions-interactions-001 | AI + MedicalRecords | Backend |
| dev-5 | front-messaging-conversation-001 | Messaging | Frontend |
| dev-6 | front-prescriptions-catalog-001 | Prescriptions | Frontend |
| dev-7 | front-messaging-admin-settings-001 | Messaging | Frontend |
| dev-8 | front-messaging-owner-portal-001 | Messaging | Frontend |

## État global

### Modules complétés (DONE: 99)
- Auth (login, refresh, team management, RBAC, change password, clinic self-registration)
- Agenda (CRUD, status transitions, availability, appointments analytics, reminders)
- MedicalRecords (patients, owners, medical records, prescriptions, CSV import)
- Billing (invoices, items, PDF generation, revenue analytics)
- Notifications (email templates, consumers)
- AI scaffold (triage, no-show prediction)
- Stock (scaffold + management)
- Infrastructure (Docker, CI/CD, HTTPS, monitoring, rate limiting, audit, secrets)
- Frontend (scaffold, layout, auth, patients, billing, appointments, dashboard, landing, i18n)

### Messaging (NEW — vague 2 en cours)

| Task | Statut | Notes |
|---|---|---|
| back-messaging-domain-001 | DONE | Entités, DbContext, migrations |
| back-messaging-hours-001 | DONE | Business hours config |
| back-messaging-staff-handlers-001 | WIP | 3 queries + 7 commands + 10 endpoints |
| back-messaging-owner-portal-001 | WIP | Magic link auth + 7 endpoints |
| back-messaging-templates-001 | WIP | CRUD templates EN+AR |
| back-messaging-sse-001 | TODO | SSE real-time |
| back-messaging-ai-integration-001 | TODO | AI triage + suggestions |
| back-messaging-notifications-001 | TODO | Email notifications |
| back-messaging-agenda-integration-001 | TODO | Convert to appointment |
| back-messaging-medical-integration-001 | TODO | Attach to medical record |
| front-messaging-conversation-001 | WIP | Conversation detail page |
| front-messaging-admin-settings-001 | WIP | Admin settings UI |
| front-messaging-owner-portal-001 | WIP | Owner portal pages |
| front-messaging-realtime-001 | TODO | SSE client |
| front-messaging-playwright-001 | TODO | E2E tests |

### Prescriptions AI (vague 2 en cours)

| Task | Statut | Notes |
|---|---|---|
| back-prescriptions-catalog-001 | DONE | DrugCatalogEntry seeded |
| back-prescriptions-enrich-001 | DONE | Prescription enrichment |
| back-prescriptions-weight-001 | DONE | Patient.WeightKg |
| back-prescriptions-interactions-001 | WIP | Drug interaction checker |
| back-prescriptions-override-001 | TODO | Override with audit |
| back-prescriptions-stock-link-001 | TODO | Stock decrement on dispense |
| back-prescriptions-stock-suggest-001 | TODO | Stock availability suggest |
| front-prescriptions-catalog-001 | WIP | Drug autocomplete UI |
| front-prescriptions-alerts-001 | TODO | Interaction alerts UI |
| front-prescriptions-stock-001 | TODO | Stock link UI |
| front-prescriptions-e2e-001 | TODO | E2E tests |

### Tâches restantes (TODO)

| Task | Dépendances | Notes |
|---|---|---|
| agenda-002 | agenda-001 | — |
| medical-002 | medical-001 | — |
| back-api-public-001 | — | Public API |
| back-multi-clinic-001 | — | Multi-clinic study |
| front-layout-001 | front-scaffold | — |
| front-auth-001 | front-scaffold | — |
| front-agenda-001 | front-layout + back-agenda | — |
| front-billing-001 | front-layout + back-billing | — |
| front-medical-001 | front-layout + back-medical | — |

## Questions en attente

- `questions/whatsapp-integration-001.md` — Étude WhatsApp Business API (PO + Archi)
- `questions/drug-catalog-placement-001.md` — Placement DrugCatalogEntry (décidé: MedicalRecords)
