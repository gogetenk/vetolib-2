# todo-back-billing-invoice-fields-003 -- Champs obligatoires facturation electronique

**Module** : Billing
**Priorite** : Critique
**Prerequis** : Aucun (parallelisable avec 001, 002)
**Skills** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`
**Etude** : `docs/studies/E-INVOICING-FRANCE-STUDY-2026.md`

---

## Contexte

La facturation electronique francaise (et la norme EN16931) impose des champs supplementaires absents du modele actuel.

## Champs a ajouter

### Sur Invoice (entite Domain)

| Champ | Type | Obligatoire | Description |
|---|---|---|---|
| `SellerSiren` | string | Oui (FR) | SIREN de la clinique (9 chiffres) |
| `SellerVatNumber` | string | Oui (FR) | N TVA intracommunautaire (FRxx + 9 chiffres) |
| `BuyerSiren` | string? | Si B2B | SIREN de l'acheteur professionnel |
| `BuyerVatNumber` | string? | Si B2B | N TVA de l'acheteur |
| `BuyerName` | string | Oui | Nom du client (personne ou entreprise) |
| `BuyerAddress` | string? | Si B2B | Adresse de l'acheteur |
| `OperationType` | enum | Oui (FR) | Service, Goods, Mixed |
| `InvoiceTypeCode` | string | Oui | "380" (facture), "381" (avoir) |
| `PaymentTerms` | string? | Non | Conditions de paiement texte |
| `CountryCode` | string | Oui | ISO 3166-1 alpha-2 (AE, FR, PL) |
| `PurchaseOrderReference` | string? | Non | Reference bon de commande |

### Sur InvoiceDto (Contracts)

Memes champs exposes dans le DTO.

## Travail a faire

- [ ] Creer l'enum `OperationType` dans `Billing.Contracts` : `Service`, `Goods`, `Mixed`
- [ ] Ajouter les champs au Domain `Invoice`
- [ ] Ajouter les champs au `InvoiceDto`
- [ ] Mettre a jour `CreateInvoiceCommand` et `CreateInvoiceRequest` avec les nouveaux champs optionnels
- [ ] Validation : si `CountryCode == "FR"`, les champs SIREN et TVA du vendeur sont obligatoires
- [ ] Migration EF Core
- [ ] Mettre a jour le PDF generator avec les mentions legales

## Critere de completion

- [ ] Les champs sont presents dans le Domain, le DTO et la base
- [ ] La validation conditionelle par pays fonctionne
- [ ] Les cliniques UAE ne sont pas impactees (champs optionnels)
- [ ] `dotnet build` + `dotnet test` GREEN avant commit
