# todo-nightly-loop-003.md — Boucle nocturne d'auto-amélioration

**Module** : Meta
**Dépendances** : todo-refacto-nightly-006 à 010 doivent être done
**Priorité** : HAUTE
**Skills à lire** : aucun

---

## Objectif

Cycle 3 de la boucle récursive. Scanner le codebase sur l'axe **Sécurité**.

## Cycle courant : Cycle 3 — Sécurité

- Endpoints sans `.RequireAuthorization()` qui devraient en avoir
- Données sensibles loggées (mots de passe, tokens dans les logs — vérifier `_logger.Log*` avec des arguments suspects)
- Rate limiting manquant sur des endpoints critiques (login, register, invite)
- Tokens ou secrets hardcodés dans le code source (hors appsettings et tests)

## Rappel des axes futurs

### Cycle 4 — Tests
- Handlers sans test unitaire correspondant
- Scénarios Gherkin @wip oubliés
- Assertions faibles dans les tests existants

### Cycle 5 — Performance & cleanup
- Usings inutiles
- Code commenté (> 3 lignes)
- TODO/FIXME/HACK non résolus

## Commandes de scan suggérées

```bash
# Endpoints sans RequireAuthorization
grep -rn "MapPost\|MapGet\|MapPut\|MapDelete" src/backend/Modules --include="*.cs" -A2 \
  | grep -B1 "Map" | grep -v "RequireAuthorization\|AllowAnonymous" | head -20

# Logging avec données sensibles
grep -rn "_logger.Log" src/backend --include="*.cs" -A1 \
  | grep -i "password\|token\|secret\|pwd" | head -10

# Rate limiting — vérifier les endpoints Auth
grep -rn "RequireRateLimiting\|WithRateLimiting" src/backend/Modules/Auth --include="*.cs" | head -10
```

## Règles

- **Maximum 5 tâches par cycle** — pas de flood
- **Chaque tâche créée doit être actionnable**
- **Les tâches refacto ne touchent qu'un module à la fois**
- **Ne jamais modifier Shared/ sans autorisation**

## Critères de complétion

```
□ Scan effectué sur l'axe du cycle courant (Sécurité)
□ Tâches créées pour les améliorations trouvées (max 5)
□ Si plus rien à trouver → ne pas créer de successeur
□ Si des tâches créées → créer todo-nightly-loop-004.md pour le prochain cycle
□ Renommer en done-nightly-loop-003.md
```
