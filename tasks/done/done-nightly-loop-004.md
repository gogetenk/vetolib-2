# todo-nightly-loop-004.md — Boucle nocturne d'auto-amélioration

**Module** : Meta
**Dépendances** : done-nightly-loop-003.md
**Priorité** : HAUTE
**Skills à lire** : aucun

---

## Objectif

Cycle 4 de la boucle récursive. Scanner le codebase sur l'axe **Tests**.

## Cycle courant : Cycle 4 — Tests

- Handlers sans test unitaire correspondant (comparer la liste des handlers vs les fichiers de test)
- Scénarios Gherkin marqués @wip ou @ignore oubliés
- Assertions faibles dans les tests existants (ex: `Assert.NotNull` sans vérification du contenu)
- Handlers de commandes critiques (Create, Update, Delete) sans test de validation (Validator sans test)

## Commandes de scan suggérées

```bash
# Handlers sans test
find src/backend/Modules -name "*Handler.cs" | sed 's|.*Commands/||;s|.*Queries/||;s|/.*||' | sort
find tests/Vetolib.Tests.Unit -name "*Tests.cs" | sed 's|.*Tests.Unit/||;s|Tests.cs||' | sort

# Scénarios wip/ignore
grep -rn "@wip\|@ignore\|@skip" tests/Vetolib.Tests.Acceptance/Features/ --include="*.feature"

# Validators sans test
find src/backend/Modules -name "*Validator.cs" | xargs -I{} basename {} Validator.cs
find tests/Vetolib.Tests.Unit -name "*ValidatorTests.cs" | xargs -I{} basename {} ValidatorTests.cs
```

## Rappel des axes futurs

### Cycle 5 — Performance & cleanup
- Usings inutiles
- Code commenté (> 3 lignes)
- TODO/FIXME/HACK non résolus

## Règles

- **Maximum 5 tâches par cycle** — pas de flood
- **Chaque tâche créée doit être actionnable**
- **Les tâches de test ne touchent qu'un module à la fois**
- **Ne jamais modifier Shared/ sans autorisation**

## Critères de complétion

```
□ Scan effectué sur l'axe du cycle courant (Tests)
□ Tâches créées pour les lacunes trouvées (max 5)
□ Si plus rien à trouver → ne pas créer de successeur
□ Si des tâches créées → créer todo-nightly-loop-005.md pour le prochain cycle
□ Renommer en done-nightly-loop-004.md
```
