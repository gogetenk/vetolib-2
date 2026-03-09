# progress.md — Vetolib

_Mis à jour par l'orchestrator à chaque cycle._

## 2026-03-09 — Cycle orchestrateur
- TODO: 7 | WIP: 5 | DONE: 2
- Agents actifs : [wip-auth-002, wip-agenda-001, wip-billing-001, wip-medical-001, wip-front-scaffold-000]
- PRs en review : 0
- Questions PO : 0
- Prochaine action : 5 agents dispatch — backend tests BDD + frontend scaffold

## État global

### Backend (8 tâches)

| Task | Statut | Dépendances | Notes |
|---|---|---|---|
| scaffold-000 | DONE | — | Solution compile |
| auth-001 | DONE | scaffold-000 | Login/Refresh implémenté |
| auth-002 | WIP | auth-001 | Code existe, tests manquants |
| agenda-001 | WIP | auth-001 | Code existe, tests manquants |
| agenda-002 | TODO | agenda-001 | — |
| medical-001 | WIP | auth-001 | Code existe, tests manquants |
| medical-002 | TODO | medical-001 | — |
| billing-001 | WIP | auth-001 | Code existe, tests manquants |

### Frontend (6 tâches)

| Task | Statut | Dépendances | Notes |
|---|---|---|---|
| front-scaffold-000 | WIP | aucune | Next.js à créer |
| front-layout-001 | TODO | front-scaffold-000 | — |
| front-auth-001 | TODO | front-scaffold-000 | [MSW: oui] |
| front-agenda-001 | TODO | front-layout-001 + back-agenda-001 | [MSW: oui] |
| front-medical-001 | TODO | front-layout-001 + back-medical-001 | [MSW: oui] |
| front-billing-001 | TODO | front-layout-001 + back-billing-001 | [MSW: oui] |

## Graphe de dépendances

```
ROUND 1 (DONE) :
  [back-scaffold-000 ✓]   [front-scaffold-000 → WIP]

ROUND 2 (EN COURS — 4 agents) :
  [back-auth-002 → WIP]    [front-layout-001 → attend front-scaffold]
  [back-agenda-001 → WIP]
  [back-medical-001 → WIP]
  [back-billing-001 → WIP]

ROUND 3 (BLOQUÉ) :
  [back-agenda-002]        [front-auth-001]
  [back-medical-002]       [front-agenda-001]
                           [front-medical-001]
                           [front-billing-001]
```
