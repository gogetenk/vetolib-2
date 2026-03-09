# todo-infra-version-bump-001.md — Version bump global NuGet + SDKs

**Module** : Infra
**Dépendances** : aucune
**Priorité** : HAUTE (maintenance)

---

## Objectif

Mettre à jour tous les packages NuGet et SDKs à leurs dernières versions stables. Vérifier que le build et les tests passent après la mise à jour.

## Implémentation

### 1. Identifier les versions actuelles

```bash
# Lister tous les packages outdated
dotnet list Vetolib.sln package --outdated
```

### 2. Packages à bumper

#### SDKs / Framework
- `Microsoft.NET.Sdk` → vérifier `global.json` (target .NET 10 latest)
- `Microsoft.AspNetCore.*` → latest 10.x
- `Microsoft.Extensions.*` → latest 10.x

#### Aspire
- `Aspire.Hosting.*` → latest 9.x
- `Aspire.Components.*` → latest 9.x

#### Core dependencies
- `MediatR` → latest
- `FluentValidation` → latest
- `FluentValidation.DependencyInjectionExtensions` → latest
- `Ardalis.Result` → latest
- `Ardalis.Result.AspNetCore` → latest

#### EF Core
- `Microsoft.EntityFrameworkCore` → latest 10.x
- `Npgsql.EntityFrameworkCore.PostgreSQL` → latest compatible
- `Microsoft.EntityFrameworkCore.Design` → latest 10.x

#### Testing
- `xunit` → latest
- `Reqnroll` → latest
- `Testcontainers` → latest
- `NSubstitute` → latest
- `Microsoft.AspNetCore.Mvc.Testing` → latest 10.x

#### Infrastructure
- `Serilog.*` → latest
- `MassTransit` → latest
- `QuestPDF` → latest
- `CsvHelper` → latest (si installé)

#### Frontend (npm)
- `next` → latest 15.x
- `react` / `react-dom` → latest 19.x
- `typescript` → latest 5.x
- `tailwindcss` → latest 4.x
- `playwright` → latest
- Tous les `@radix-ui/*` → latest
- `next-intl` → latest
- `recharts` → latest (si installé)

### 3. Process

1. `dotnet list Vetolib.sln package --outdated` → noter les versions
2. Bumper chaque `.csproj` (utiliser `dotnet add package` ou éditer directement)
3. `dotnet build Vetolib.sln` → fixer les breaking changes
4. `dotnet test` → vérifier que les tests passent
5. `cd src/frontend && npm update && npm audit fix`
6. `npm run build` → vérifier le build frontend
7. Mettre à jour `global.json` si nécessaire

### 4. Breaking changes à surveiller

- MediatR 13+ : changement d'API des pipelines
- FluentValidation 12+ : namespace changes
- Aspire 9.2+ : API hosting changes
- Next.js 15.x : App Router changes
- Tailwind 4.x : config changes

## Critère

```
□ dotnet list package --outdated → 0 packages outdated (ou justification pour chaque skip)
□ dotnet build → 0 errors
□ dotnet test → tous verts
□ npm outdated → 0 outdated (ou justification)
□ npm run build → success
□ global.json à jour
□ Renommer en done
```
