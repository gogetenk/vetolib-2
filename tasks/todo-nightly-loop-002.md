# todo-nightly-loop-002.md — Boucle nocturne d'auto-amélioration

**Module** : Meta
**Dépendances** : todo-refacto-nightly-001 à 005 doivent être done
**Priorité** : HAUTE
**Skills à lire** : aucun

---

## Objectif

Cycle 2 de la boucle récursive. Scanner le codebase sur l'axe **Robustesse** (voir axes ci-dessous).

## Cycle courant : Cycle 2 — Robustesse

- Try/catch silencieux (catch vide ou avec juste un log sans retourner d'erreur)
- Requêtes EF sans pagination (`.ToListAsync()` sans `.Take()`) — nouveaux handlers ajoutés depuis Cycle 1
- Requêtes N+1 (boucle avec query dedans)
- Missing `.AsNoTracking()` sur les queries read-only — modules AI et Messaging non encore couverts

## Rappel des axes futurs

### Cycle 3 — Sécurité
- Endpoints sans `.RequireAuthorization()` qui devraient en avoir
- Données sensibles loggées (mots de passe, tokens dans les logs)
- Rate limiting manquant sur certains endpoints

### Cycle 4 — Tests
- Handlers sans test unitaire correspondant
- Scénarios Gherkin @wip oubliés
- Assertions faibles

### Cycle 5 — Performance & cleanup
- Usings inutiles
- Code commenté (> 3 lignes)
- TODO/FIXME/HACK non résolus

## Règles

- **Maximum 5 tâches par cycle** — pas de flood
- **Chaque tâche créée doit être actionnable**
- **Les tâches refacto ne touchent qu'un module à la fois**
- **Ne jamais modifier Shared/ sans autorisation**

## Critères de complétion

```
□ Scan effectué sur l'axe du cycle courant (Robustesse)
□ Tâches créées pour les améliorations trouvées (max 5)
□ Si plus rien à trouver → ne pas créer de successeur
□ Si des tâches créées → créer todo-nightly-loop-003.md pour le prochain cycle
□ Renommer en done-nightly-loop-002.md
```
