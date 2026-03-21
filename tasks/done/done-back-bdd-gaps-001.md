# todo-back-bdd-gaps-001.md — Combler les gaps BDD (Dashboard + Audit + edge cases)

**Module** : Tests
**Dépendances** : aucune
**Priorité** : HAUTE (BDD audit — 8 endpoints sans Gherkin)

---

## Contexte

L'audit BDD a identifié 8 endpoints sans scénario Gherkin (25% de découvert). Les endpoints Dashboard et Audit n'ont aucun test BDD, ce qui viole la règle BDD-first.

## Périmètre

### 1. Dashboard feature (3 endpoints)
Créer `features/dashboard/dashboard.feature` + step definitions :

```gherkin
Feature: Dashboard statistics
  Scenario: Admin sees all dashboard stats
    Given I am logged in as admin
    When I request dashboard stats
    Then I see appointments today count
    And I see pending checkin count
    And I see unpaid invoices total in AED
    And I see total patients count

  Scenario: Today appointments list
    Given I am logged in as vet
    And there are 3 appointments today
    When I request today's appointments
    Then I see 3 appointments sorted by time

  Scenario: Recent activity feed
    Given I am logged in as admin
    When I request recent activity
    Then I see the latest actions across all modules
```

### 2. Audit feature (1 endpoint)
Créer `features/audit/audit.feature` + step definitions :

```gherkin
Feature: Audit trail
  Scenario: Admin can query audit log
    Given I am logged in as admin
    And a patient was created
    When I query audit for entityType "Patient"
    Then I see an audit entry with action "Created"
    And the entry contains the user email

  Scenario: Non-admin cannot access audit
    Given I am logged in as vet
    When I query audit
    Then I receive 403 Forbidden
```

### 3. Edge cases importants
- Billing : montants négatifs rejetés, arrondi TVA correct (115.33 × 5%)
- Appointments : modification RDV existant (PATCH)
- Auth : email case-insensitive (John@Test.com = john@test.com)

## Critère
```
□ dashboard.feature créé — 3+ scénarios
□ audit.feature créé — 2+ scénarios (ADMIN only + 403)
□ Step definitions implémentées et GREEN
□ Edge cases billing/auth ajoutés
□ Couverture endpoints passe de 75% à 95%+
□ Renommer en done
```
