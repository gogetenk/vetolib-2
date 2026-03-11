# todo-test-weak-assertions-001 — Renforcer les assertions faibles dans les tests

**Module** : Cross-module (Agenda BDD steps + Audit unit tests)
**Priorité** : MOYENNE
**Skills à lire** : aucun

---

## Contexte

Le scan de qualité des tests a révélé deux catégories de problèmes d'assertions :

### 1. Assertions terminales trop faibles dans les step definitions BDD (Agenda)

Dans `tests/Vetolib.Tests.Acceptance/StepDefinitions/Agenda/AppointmentSteps.cs`, plusieurs steps utilisent `.Should().NotBeNull()` comme **seule assertion** sans vérifier le contenu :

- Ligne 437 : `appointments.Should().NotBeNull()` — ne vérifie pas que la liste contient des éléments
- Ligne 450 : `appointments.Should().NotBeNull()` — idem
- Ligne 503 : `_availabilitySlots.Should().NotBeNull()` — ne vérifie pas que les slots ont des propriétés valides (StartTime, EndTime)
- Ligne 513 : `_availabilitySlots.Should().NotBeNull()` — idem

Ces steps passent en vert même si l'API retourne une liste vide ou un objet avec toutes les valeurs nulles.

### 2. Assertion finale inutile dans AuditInterceptorTests

Dans `tests/Vetolib.Tests.Unit/Audit/AuditInterceptorTests.cs`, ligne 94 :
```csharp
interceptor.Should().NotBeNull();
```
Cette assertion vérifie que l'objet instancié dans le même test n'est pas null — ce qui est trivial et ne valide aucun comportement.

## Travail à faire

### Fix 1 — AppointmentSteps.cs

Remplacer les assertions `NotBeNull()` terminales par des assertions sur le contenu :

```csharp
// Avant
appointments.Should().NotBeNull();

// Après — exemple pour une liste
appointments.Should().NotBeNull().And.NotBeEmpty();

// Après — exemple pour les slots
_availabilitySlots.Should().NotBeNull()
    .And.NotBeEmpty()
    .And.OnlyContain(s => s.StartTime < s.EndTime);
```

### Fix 2 — AuditInterceptorTests.cs

Supprimer la ligne 94 (`interceptor.Should().NotBeNull();`) et la remplacer par une assertion sur un comportement observable (ex: vérifier que `interceptor` implémente `ISaveChangesInterceptor`, ou supprimer simplement cette assertion redondante si elle n'apporte rien).

## Critères de complétion

```
□ Les 4 occurrences de AppointmentSteps.cs remplacées par des assertions de contenu
□ L'assertion triviale de AuditInterceptorTests.cs supprimée ou remplacée
□ dotnet test tests/Vetolib.Tests.Acceptance/ → tous les tests existants restent VERTS
□ dotnet test tests/Vetolib.Tests.Unit/ → 0 régression
```
