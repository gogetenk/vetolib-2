# todo-test-billing-pdf-001.md — Couverture Gherkin pour GET invoice PDF

**Module** : Billing
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `ardalis-result`, `reqnroll-bindings`

---

## Contexte

L'audit `done-audit-coverage-001.md` a révélé que `GET /api/v1/invoices/{id}/pdf` est implémenté dans `InvoiceEndpoints.cs` (handler `DownloadInvoicePdf`) mais ne dispose d'**aucun scénario Gherkin**.

## Travail à faire

### Étape 1 — Ajouter des scénarios dans Facturation.feature

Fichier : `tests/Vetolib.Tests.Acceptance/Features/Billing/Facturation.feature`

Ajouter à la fin du fichier :

```gherkin
Scenario: Télécharger le PDF d'une facture envoyée
  Given une facture "SENT" pour "Max" avec au moins un item
  When je télécharge le PDF de cette facture
  Then la réponse a le statut 200
  And le Content-Type est "application/pdf"
  And le contenu n'est pas vide

Scenario: Impossible de télécharger le PDF d'une facture brouillon
  Given une facture "DRAFT" pour "Max"
  When je télécharge le PDF de cette facture
  Then la réponse a le statut 400

Scenario: PDF inexistant retourne 404
  When je télécharge le PDF d'une facture avec un ID aléatoire inexistant
  Then la réponse a le statut 404
```

### Étape 2 — Bindings Reqnroll (RED d'abord)

Fichier : `tests/Vetolib.Tests.Acceptance/StepDefinitions/Billing/FacturationSteps.cs`

Ajouter les steps manquants. Vérifier que les tests sont ROUGES.

### Étape 3 — Vérifier GREEN

```bash
dotnet test tests/Vetolib.Tests.Acceptance/ --filter "Feature[Billing]"
dotnet build src/backend/Vetolib.sln
```

## Règles métier

- Seules les factures avec statut SENT ou PAID peuvent générer un PDF (pas DRAFT)
- Le PDF doit contenir : numéro de facture, nom clinique, liste items, TVA 5%, total en AED
- L'accès est restreint aux utilisateurs authentifiés de la clinique (tenant isolation)

## Critères de complétion

```
[ ] 3 scénarios Gherkin ajoutés dans Facturation.feature
[ ] Steps Reqnroll écrits et ROUGES confirmés
[ ] Tous les scénarios VERTS
[ ] dotnet build -> 0 erreur
```
