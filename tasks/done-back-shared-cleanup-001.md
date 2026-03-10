# todo-back-shared-cleanup-001.md — Dédupliquer ValidationBehavior + cleanup Shared

**Module** : Shared + tous les modules
**Dépendances** : aucune
**Priorité** : NORMALE
**MODIF_SHARED: autorisé** (décision architecte)

---

## Contexte

`ValidationBehavior<TRequest, TResponse>` est copié-collé identique dans 4 modules (Auth, Agenda, MedicalRecords, Billing). C'est de l'infrastructure pure (pas de logique métier) — il peut être mutualisé dans Shared.

## Périmètre

### 1. Déplacer ValidationBehavior dans Shared

Créer `Shared/Vetolib.Shared.Infrastructure/Behaviors/ValidationBehavior.cs` :

```csharp
namespace Vetolib.Shared.Infrastructure.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage, "", ValidationSeverity.Error)).ToList();
            // Return Result.Invalid si TResponse est un Result
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // ... pattern existant dans chaque module
            }
            return (TResponse)(object)Result.Invalid(errors);
        }

        return await next();
    }
}
```

### 2. Supprimer les 4 copies

Supprimer :
- `Modules/Auth/Vetolib.Auth/Application/Behaviors/ValidationBehavior.cs`
- `Modules/Agenda/Vetolib.Agenda/Application/Behaviors/ValidationBehavior.cs`
- `Modules/MedicalRecords/Vetolib.MedicalRecords/Application/Behaviors/ValidationBehavior.cs`
- `Modules/Billing/Vetolib.Billing/Application/Behaviors/ValidationBehavior.cs`

### 3. Mettre à jour les ModuleServiceRegistrars

Chaque module enregistre `AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))`.
Mettre à jour le namespace pour pointer vers `Vetolib.Shared.Infrastructure.Behaviors`.

### 4. Ajouter les NuGet nécessaires à Shared.Infrastructure

Si pas déjà présents : `MediatR`, `FluentValidation`, `Ardalis.Result`

## Critère

```
□ ValidationBehavior unique dans Shared.Infrastructure
□ 4 copies supprimées des modules
□ Tous les modules utilisent la version Shared
□ dotnet build → 0 erreur
□ Tests existants toujours verts
□ Renommer en done
```
