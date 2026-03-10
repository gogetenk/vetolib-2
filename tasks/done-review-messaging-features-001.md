# todo-review-messaging-features-001.md — Review PO + Archi des features Messaging

**Type** : Review (pas de code)
**Priorité** : HAUTE (doit être fait AVANT la vague 2 de dev)
**Dépendances** : done-back-messaging-domain-001, done-back-messaging-gherkin-001
**Assignés** : Agent PO + Agent Architecte

## Objectif

Valider que le domaine Messaging implémenté et les Gherkins écrits sont conformes à la spec `docs/MESSAGING-SPEC.md`.

## Review PO
- Les entités domain couvrent-elles tous les cas d'usage de la spec ?
- Les Gherkins couvrent-ils tous les scénarios critiques ?
- Y a-t-il des edge cases métier manquants ?
- Le vocabulaire métier est-il correct (EN, pas FR dans le code) ?

## Review Architecte
- Structure 2 assemblies respectée ?
- Result<T> sur toutes les factories domain ?
- Pas de throw business ?
- Multi-tenant OK (ClinicId sur toutes les entités) ?
- Pas de référence runtime inter-modules ?
- Conventions EF Core (configurations, pas d'annotations) ?

## Critère
```
[] Review PO validée — aucun scénario métier manquant
[] Review Archi validée — 0 violation
[] Renommer en done
```
