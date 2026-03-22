# Progress

## Etude d'impact -- Facturation electronique France -- 2026-03-22

### Synthese

La reforme de la facturation electronique obligatoire en France impacte directement le module Billing de Vetolib. Les cabinets veterinaires francais devront :
- **Sept 2026** : recevoir des factures electroniques
- **Sept 2027** : emettre des factures Factur-X + e-reporting B2C

### Impact sur Vetolib : MAJEUR

Le module Billing actuel est concu pour le marche UAE (TVA 5% hardcodee, devise AED, PDF simple). Pour la France, il faut :
1. TVA multi-taux configurable (20%, 10%, 5.5%)
2. Devise configurable (EUR)
3. Champs obligatoires supplementaires (SIREN, TVA intracom, type operation)
4. Generation Factur-X (PDF/A-3 + XML CII)
5. Transmission PDP/PPF (API)
6. E-reporting B2C (80-90% des factures veterinaires)

### Taches creees : 6

| Tache | Description | Phase | Priorite |
|---|---|---|---|
| `todo-back-billing-multi-tax-001` | TVA multi-taux configurable | 1 - Fondations | Critique |
| `todo-back-billing-multi-currency-002` | Devise configurable | 1 - Fondations | Critique |
| `todo-back-billing-invoice-fields-003` | Champs obligatoires e-invoicing | 1 - Fondations | Critique |
| `todo-back-billing-facturx-gen-005` | Generateur Factur-X | 2 - Format | Critique |
| `todo-back-billing-einvoicing-gateway-007` | Interface + integration PDP | 3 - Transmission | Critique |
| `todo-back-billing-ereporting-009` | E-reporting B2C | 3 - Transmission | Critique |

### Questions ouvertes : 2

| Question | Fichier | Bloquant |
|---|---|---|
| Choix PDP partenaire | `questions/billing-pdp-choice-001.md` | Non (Phase 3) |
| Taux TVA veterinaires exacts | `questions/billing-french-vat-rates-001.md` | Oui (Phase 1) |

### Decisions architecturales

- Format : **Factur-X EN16931** (PDF/A-3 + XML CII)
- Role Vetolib : **OD** (Operateur de Dematerialisation), pas PDP
- Pattern : **IEInvoicingGateway** abstrait dans Contracts, multi-PDP
- Pas de modification de `Shared/` -- tout reste dans le module Billing
- Feature flag par clinique (`EInvoicingEnabled`)

### Etude complete

`docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

### Timeline recommandee

- **Avril-Mai 2026** : Phase 1 (fondations multi-taux, multi-devise, champs)
- **Juin-Juillet 2026** : Phase 2 (Factur-X)
- **Juillet-Aout 2026** : Phase 3 (transmission PDP, e-reporting)
- **1er Sept 2026** : deadline reception factures electroniques

---

## Audit archi -- 2026-03-21

### Violations critiques : 2 (taches refacto creees)
1. **todo-refacto-20260321-002** -- IgnoreQueryFilters abuse dans Messaging (15+ occurrences portal handlers)
2. **todo-refacto-20260321-003** -- Outbox MassTransit manquant dans Messaging, Notifications, Billing

### Violations importantes : 5 (taches refacto creees)
3. **todo-refacto-20260321-001** -- French strings dans SlotScoringService (10 strings)
4. **todo-refacto-20260321-004** -- 14+ constantes hardcodees (VAT, lock duration, trial, etc.)
5. **todo-refacto-20260321-005** -- Patient.SetWeight()/AddOwner() retournent void au lieu de Result
6. **todo-refacto-20260321-007** -- Circuit Breaker manquant sur WhatsApp/LLM/SMTP
7. **todo-refacto-20260321-008** -- Messaging.Contracts reference transitive MedicalRecords.Contracts

### Violations mineures : 1 (tache refacto creee)
8. **todo-refacto-20260321-006** -- Conversation.ChangeCategory/UpdateCategory dupliques

### Constats positifs
- Zero Controller (Minimal APIs uniquement) -- CONFORME
- Zero reference cross-runtime entre modules -- CONFORME
- Result<T> sur tous les handlers -- CONFORME
- Aucun lazy loading accidentel -- CONFORME
- Features Gherkin 100% en anglais -- CONFORME
- Zero technique dans les .feature -- CONFORME
- Domain model riche avec state machines (Appointment, Invoice, Conversation) -- BON

### Dette test
- 53/85 handlers backend sans test unitaire (62% non couverts en TU)
- Module Messaging : 75% des handlers sans TU
- Module Notifications : 100% des handlers sans TU (seulement domain tests)

### Patterns avances -- recommandations
- **Saga MassTransit** : pas necessaire au stade MVP. Candidat futur si BNPL (Tabby) integre.
- **Event Sourcing** : trop complexe, l'audit interceptor existant suffit. Alternative : event log table.
- **CQRS Read Models** : pertinent pour le dashboard (5 queries live -> 1 vue materialisee).
- **Outbox Pattern** : deja en place pour Agenda/Auth, manquant pour 3 modules (tache creee).
- **Circuit Breaker** : necessaire pour WhatsApp/LLM/SMTP (tache creee).
- **Feature Flags** : le module Preferences couvre le besoin actuel.

### Conformite globale : ATTENTION
- Architecture : solide, bien isolee
- Domain : riche, quelques lacunes mineures
- Tests : couverture TU insuffisante (priorite elevee)
- Configuration : trop de hardcoded values
- Resilience : pas de circuit breaker sur les appels externes

---

## Architecture Predictive Health Alerts -- 2026-03-22

### Decision

Predictive Health Alerts Phase 1 (rules engine, pas de ML) vit dans le **module AI existant**.
Pas de nouveau module -- le module AI gere deja triage, no-show, SOAP notes et drug interactions.

### Entites

- `HealthAlert` : entity multi-tenant avec Status (New/Acknowledged/Scheduled/Dismissed), Severity, RuleId, lien optionnel vers appointment
- `BreedRiskData` : dictionnaire statique en C# (pas d'entite EF en Phase 1)

### Rules Engine (10 regles Phase 1)

| RuleId | Declencheur | Alerte |
|---|---|---|
| BREED-RENAL-001 | Chat >= 7a, pas de bilan renal 12m | Renal screening overdue |
| BREED-CARDIAC-001 | CKCS/Doberman/Boxer >= 5a, pas de cardiac 12m | Cardiac screening recommended |
| SENIOR-WELLNESS-001 | Chien >= 8a ou Chat >= 10a, pas de wellness 6m | Senior wellness exam recommended |
| WEIGHT-TREND-001 | Prise de poids > 15% sur 3 visites | Weight gain trend detected |
| VACCINE-GAP-001 | Vaccination core en retard > 30j | Core vaccination overdue |
| BRACHY-RESP-001 | Race brachycephale + historique respiratoire | Airway assessment recommended |
| BREED-HIP-001 | Golden/Lab/GSD >= 2a, pas de radio hanche | Hip dysplasia screening recommended |
| DIABETES-RISK-001 | Chat en surpoids, >= 5a | Obesity screening recommended |
| DENTAL-001 | Chien/Chat >= 3a, pas de detartrage 24m | Dental prophylaxis recommended |
| ARTHRITIS-001 | Grand chien >= 7a, arthrite en historique, pas de suivi 4m | Arthritis review due |

### Impact inter-modules

- **MedicalRecords.Contracts** : nouveau `IPatientAlertDataReader` (lecture patients + historique poids)
- **Agenda** : pas de couplage direct. Le frontend pre-remplit le formulaire RDV via les donnees de l'alerte.
- **Shared/** : ZERO modification

### Taches creees : 5

| Tache | Description | Estimee |
|---|---|---|
| `todo-back-predictive-health-001` | Domain + Rules Engine + contrats | 2-3h |
| `todo-back-predictive-health-002` | Implementation IPatientAlertDataReader dans MedicalRecords | 1-2h |
| `todo-back-predictive-health-003` | Endpoints API + Handlers CQRS | 2-3h |
| `todo-back-predictive-health-004` | Background job + generation handler | 2h |
| `todo-front-predictive-health-001` | Dashboard + patient alerts (MSW) | 3-4h |

### Parallelisation

- Taches 001 et 002 en parallele (contrat dans 001, implementation dans 002)
- Tache 003 depend de 001
- Tache 004 depend de 001 + 002
- Tache frontend 001 demarre immediatement avec MSW (zero dependance backend)

### Spec complete

`docs/specs/PREDICTIVE-HEALTH-ALERTS-SPEC.md`
