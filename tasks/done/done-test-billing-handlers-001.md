# todo-test-billing-handlers-001 — Unit tests for Billing command handlers

**Module** : Billing
**Priorité** : HAUTE
**Skills à lire** : `skills/ardalis-result/SKILL.md`, `skills/cqrs-mediatr/SKILL.md`

---

## Contexte

Le module Billing ne possède aucun test unitaire sur les handlers, seulement `InvoicePdfTests.cs` (génération PDF). Les 3 command handlers critiques sont non couverts :

- `CreateInvoiceHandler`
- `AddInvoiceItemHandler`
- `UpdateInvoiceStatusHandler`

Aucun validator de ce module n'est non plus testé (`CreateInvoiceValidator`, `AddInvoiceItemValidator`, `UpdateInvoiceStatusValidator`).

## Travail à faire

Créer `tests/Vetolib.Tests.Unit/Billing/CreateInvoiceHandlerTests.cs` couvrant :
- Happy path : facture créée avec statut `Draft`, ID retourné
- PatientId inexistant : `Result.NotFound`
- Facture déjà existante pour même rendez-vous (si règle business) : `Result.Invalid`

Créer `tests/Vetolib.Tests.Unit/Billing/AddInvoiceItemHandlerTests.cs` couvrant :
- Happy path : ligne ajoutée, total recalculé
- Facture introuvable : `Result.NotFound`
- Ajout de ligne sur facture `Paid` : `Result.Invalid`

Créer `tests/Vetolib.Tests.Unit/Billing/UpdateInvoiceStatusHandlerTests.cs` couvrant :
- Transition Draft → Sent
- Transition Sent → Paid
- Transition invalide (Paid → Draft) : `Result.Invalid`
- Facture introuvable : `Result.NotFound`

Créer `tests/Vetolib.Tests.Unit/Billing/CreateInvoiceValidatorTests.cs` couvrant :
- Commande valide passe la validation
- PatientId vide : erreur de validation
- Amount négatif : erreur de validation

## Contraintes techniques

- InMemory EF Core avec ClinicId fixe `11111111-1111-1111-1111-111111111111`
- NSubstitute pour `IPublisher`
- Zéro Testcontainers

## Critères de complétion

```
□ CreateInvoiceHandlerTests.cs créé avec minimum 3 scénarios
□ AddInvoiceItemHandlerTests.cs créé avec minimum 3 scénarios
□ UpdateInvoiceStatusHandlerTests.cs créé avec minimum 4 scénarios
□ CreateInvoiceValidatorTests.cs créé avec minimum 3 scénarios
□ dotnet test tests/Vetolib.Tests.Unit/ → 0 erreur, 0 échec
```
