# todo-nightly-loop-001.md — Boucle nocturne d'auto-amélioration

**Module** : Meta
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : aucun

---

## Objectif

Cette tâche est une **boucle récursive**. Quand elle est terminée, elle crée de nouvelles tâches et se recrée elle-même. Le cycle ne s'arrête que quand il n'y a plus rien à améliorer.

## Cycle (à répéter)

### Phase 1 — Analyse (créer des tâches)
1. Scanner le codebase pour trouver des améliorations possibles
2. Pour chaque amélioration, créer une tâche `todo-refacto-*` ou `todo-fix-*`
3. Passer en done

### Phase 2 — Le forge dispatch les tâches créées
L'orchestrator `/forge` (cron 15min) détecte les nouvelles tâches et dispatch des agents dev.

### Phase 3 — Quand toutes les tâches sont done, relancer Phase 1
L'orchestrator recrée `todo-nightly-loop-002.md` pour un nouveau cycle d'analyse.

## Axes d'analyse (par cycle)

### Cycle 1 — Qualité structurelle
- Handlers trop longs (> 80 lignes) → extraire en méthodes privées
- Validators manquants pour des Commands
- DTOs avec trop de champs (> 10) → split
- Endpoints sans summary/description OpenAPI

### Cycle 2 — Robustesse
- Try/catch silencieux (catch vide ou avec juste un log)
- Requêtes EF sans pagination (`.ToListAsync()` sans `.Take()`)
- Requêtes N+1 (boucle avec query dedans)
- Missing `.AsNoTracking()` sur les queries read-only

### Cycle 3 — Sécurité
- Endpoints sans `.RequireAuthorization()` qui devraient en avoir
- Données sensibles loggées (mots de passe, tokens dans les logs)
- CORS trop permissif
- Rate limiting manquant sur certains endpoints

### Cycle 4 — Tests
- Handlers sans test unitaire correspondant
- Scénarios Gherkin @wip oubliés
- Tests commentés ou skippés
- Assertions faibles (pas de vérification du contenu, juste le status code)

### Cycle 5 — Performance & cleanup
- Usings inutiles
- Fichiers vides
- Code commenté (> 3 lignes)
- TODO/FIXME/HACK non résolus dans le code

### Cycle N — Quand rien trouvé
Si un cycle ne trouve aucune amélioration → marquer la tâche done sans créer de successeur. La factory s'arrête naturellement.

## Règles

- **Maximum 5 tâches par cycle** — pas de flood
- **Chaque tâche créée doit être actionnable** — pas de "améliorer la qualité" vague
- **Les tâches refacto ne touchent qu'un module à la fois**
- **Ne jamais modifier Shared/ sans autorisation**
- **Commit et push chaque fix individuellement**

## Critère de complétion

```
□ Scan effectué sur l'axe du cycle courant
□ Tâches créées pour les améliorations trouvées (max 5)
□ Si plus rien à trouver → ne pas créer de successeur
□ Si des tâches créées → créer todo-nightly-loop-002.md pour le prochain cycle
□ Renommer en done
```
