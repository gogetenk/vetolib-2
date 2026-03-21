# todo-infra-problemdetails-001.md — Global ProblemDetails exception handler

**Module** : Infra
**Dépendances** : aucune
**Priorité** : CRITIQUE (pre-prod)

---

## Objectif

Ajouter un middleware global qui catch les exceptions non gérées et retourne des réponses ProblemDetails (RFC 9457) au lieu du HTML 500 par défaut.

## Implémentation

### 1. Program.cs

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

// After build
app.UseExceptionHandler();
app.UseStatusCodePages();
```

### 2. Behavior en production

- **Development** : inclure stack trace + exception details
- **Production** : message générique "An error occurred", pas de stack trace, traceId pour corrélation

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}
```

### 3. Vérifier

- 404 retourne `application/problem+json` avec `{ "status": 404, "title": "Not Found" }`
- Exception non catchée retourne 500 avec `traceId`
- Les endpoints Ardalis.Result continuent à fonctionner normalement via `ToMinimalApiResult()`

## Critère

```
□ AddProblemDetails + UseExceptionHandler dans Program.cs
□ 404/500 retournent application/problem+json
□ traceId inclus dans la réponse
□ Pas de stack trace en production
□ Renommer en done
```
