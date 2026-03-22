# todo-back-billing-multi-tax-001 -- Taux de TVA configurable multi-pays

**Module** : Billing
**Priorite** : Critique
**Prerequis** : Aucun
**Skills** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`, `multitenant-efcore`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

Le taux de TVA est actuellement hardcode a 5% (UAE) dans `InvoiceItem.cs` :
```csharp
private const decimal TaxRate = 0.05m; // UAE VAT 5%
```

Pour le marche francais, les cabinets veterinaires appliquent potentiellement plusieurs taux sur une meme facture :
- 20% (taux normal -- actes veterinaires)
- 10% (taux reduit -- medicaments veterinaires sous conditions)
- 5.5% (alimentation animale sous conditions)

Pour la Pologne : 23% (standard), 8% (reduit).

## Objectif

Rendre le taux de TVA configurable par ligne de facture, resolu dynamiquement par pays et par categorie de produit/service.

## Travail a faire

- [ ] Creer un enum `TaxCategory` dans `Billing.Contracts` : `Standard`, `Reduced`, `SuperReduced`, `Zero`, `Exempt`
- [ ] Creer une interface `ICountryTaxResolver` dans `Billing.Contracts` :
  ```csharp
  public interface ICountryTaxResolver
  {
      decimal GetTaxRate(string countryCode, TaxCategory category);
      string GetTaxSchemeId(string countryCode); // "VAT" pour EU, "VAT" pour UAE
  }
  ```
- [ ] Implementer `CountryTaxResolver` dans le runtime Billing avec les taux pour UAE, FR, PL
- [ ] Modifier `InvoiceItem.Create()` pour accepter un `TaxCategory` et un `taxRate` explicite (plus de constante)
- [ ] Modifier `Invoice.Create()` pour accepter le `countryCode` et resoudre les taux via `ICountryTaxResolver`
- [ ] Ajouter le champ `TaxCategory` et `TaxRate` (stocke) sur `InvoiceItem` en base
- [ ] Migration EF Core pour les nouveaux champs
- [ ] Mettre a jour `InvoiceItemDto` avec `TaxCategory` et `TaxRate`
- [ ] Mettre a jour le PDF generator pour afficher le taux par ligne

## Critere de completion

- [ ] Le taux de TVA n'est plus hardcode nulle part
- [ ] Une facture peut contenir des lignes a des taux differents
- [ ] Les totaux (SubTotal, TotalTax, Total) sont corrects avec des taux mixtes
- [ ] Les tests existants passent toujours (avec taux UAE 5% par defaut)
- [ ] TU : edge cases taux multiples, taux zero, arrondi decimal
- [ ] TI : endpoint retourne le bon taux dans le DTO
- [ ] `dotnet build` + `dotnet test` GREEN avant commit
