# todo-nightly-loop-005.md — Boucle nocturne d'auto-amélioration

**Module** : Meta
**Dépendances** : done-nightly-loop-004.md
**Priorité** : BASSE
**Skills à lire** : aucun

---

## Objectif

Cycle 5 de la boucle récursive. Scanner le codebase sur l'axe **Performance & Cleanup**.

## Cycle courant : Cycle 5 — Performance & Cleanup

- Usings inutiles (fichiers avec `using` non référencés dans le code)
- Code commenté (blocs de plus de 3 lignes commentés)
- TODO/FIXME/HACK non résolus dans le code de production
- Variables ou paramètres nommés `_` ou `temp` ou `xxx`
- Méthodes async sans `await` (candidats à être synchrones)

## Commandes de scan suggérées

```bash
# TODOs/FIXMEs dans le code de production (pas les tests)
grep -rn "TODO\|FIXME\|HACK\|TEMP\|XXX" src/backend/Modules --include="*.cs" | grep -v "//.*TODO.*test" | head -30

# Méthodes async sans await
grep -rn "public async Task.*{" src/backend/Modules --include="*.cs" -A 10 | grep -v "await" | head -20

# Code commenté long (3+ lignes consécutives de //)
grep -rn "^[[:space:]]*//" src/backend/Modules --include="*.cs" | head -30
```

## Rappel des axes futurs

### Cycle 6 — Sécurité & RBAC
- Endpoints sans attribut d'autorisation
- Routes accessibles sans authentification non documentées
- Règles RBAC manquantes pour les nouveaux modules (Messaging, Stock, AI)

## Règles

- **Maximum 5 tâches par cycle** — pas de flood
- **Chaque tâche créée doit être actionnable**
- **Ne jamais modifier Shared/ sans autorisation**

## Critères de complétion

```
□ Scan effectué sur l'axe du cycle courant (Performance & Cleanup)
□ Tâches créées pour les lacunes trouvées (max 5)
□ Si plus rien à trouver → ne pas créer de successeur
□ Si des tâches créées → créer todo-nightly-loop-006.md pour le prochain cycle
□ Renommer en done-nightly-loop-005.md
```
