# todo-back-billing-multi-currency-002 -- Devise configurable par clinique

**Module** : Billing
**Priorite** : Critique
**Prerequis** : Aucun (parallelisable avec 001)
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

La devise est hardcodee "AED" dans le PDF generator :
```csharp
r.ConstantItem(120).AlignRight().Text($"{invoice.Subtotal:F2} AED");
```

Pour la France (EUR) et la Pologne (PLN), il faut que la devise soit dynamique.

## Objectif

La devise est resolue depuis la configuration de la clinique. Le module Billing ne hardcode aucune devise.

## Travail a faire

- [ ] Ajouter `CurrencyCode` (ISO 4217) dans `InvoiceDto` : `string CurrencyCode`
- [ ] Ajouter `CurrencyCode` sur l'entite `Invoice` (persiste en base)
- [ ] Resoudre la devise depuis la config clinique au moment de la creation de la facture
- [ ] Mettre a jour `InvoicePdfGenerator` pour utiliser `invoice.CurrencyCode` au lieu de "AED"
- [ ] Migration EF Core : ajout colonne `CurrencyCode` avec valeur par defaut "AED"

## Critere de completion

- [ ] "AED" n'apparait plus en dur dans le code
- [ ] Le PDF affiche la bonne devise selon la clinique
- [ ] Les factures existantes (migration) gardent "AED" comme defaut
- [ ] `dotnet build` + `dotnet test` GREEN avant commit
