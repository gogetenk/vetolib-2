# todo-refacto-nightly-003 — Stock: validator manquant pour DecrementStockByDrugCatalogEntry

**Module** : Stock
**Priorité** : HAUTE — robustesse
**Skills à lire** : `skills/cqrs-mediatr/SKILL.md`

---

## Problème

La commande `DecrementStockByDrugCatalogEntryCommand` n'a pas de validator FluentValidation. Le handler reçoit des données non validées directement.

Fichiers :
- Commande : `src/backend/Modules/Stock/Vetolib.Stock/Application/Commands/DecrementStockForPrescription/DecrementStockByDrugCatalogEntryCommand.cs`
- Handler existant : `src/backend/Modules/Stock/Vetolib.Stock/Application/Commands/DecrementStockForPrescription/DecrementStockByDrugCatalogEntryHandler.cs`
- **Validator à créer** : `src/backend/Modules/Stock/Vetolib.Stock/Application/Commands/DecrementStockForPrescription/DecrementStockByDrugCatalogEntryValidator.cs`

## Fix attendu

Créer `DecrementStockByDrugCatalogEntryValidator.cs` dans le même dossier :

```csharp
internal class DecrementStockByDrugCatalogEntryValidator : AbstractValidator<DecrementStockByDrugCatalogEntryCommand>
{
    public DecrementStockByDrugCatalogEntryValidator()
    {
        RuleFor(x => x.DrugCatalogEntryId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        // Ajouter les règles selon les champs de la commande
    }
}
```

Vérifier aussi si `DecrementStockForPrescriptionCommand` (dans Contracts) a un validator — sinon en créer un également.

## Critères de complétion

```
□ DecrementStockByDrugCatalogEntryValidator.cs créé avec règles métier complètes
□ Validator enregistré automatiquement via la behavior ValidationBehavior (pipeline MediatR)
□ dotnet build → 0 erreur
```
