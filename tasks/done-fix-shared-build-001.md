# wip-fix-shared-build-001.md — Fix Shared build error (missing FluentValidation)

**Module** : Shared
**Priorite** : CRITIQUE (build broken)
MODIF_SHARED: autorisé

## Probleme

ValidationBehavior.cs was moved from per-module to Shared/Vetolib.Shared.Infrastructure/Behaviors/ but FluentValidation package reference was not added to the csproj. Build fails with CS0246.

## Fix

Add `<PackageReference Include="FluentValidation" Version="11.*" />` to Vetolib.Shared.Infrastructure.csproj.
