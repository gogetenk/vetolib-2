# todo-merge-agenda-002.md — Débloquer PR agenda-002

**Dépendances** : aucune
**Priorité** : CRITIQUE — bloque le milestone Agenda

---

## Contexte

La PR `agenda-002` est en statut DEV_DONE mais pas mergée.
Elle bloque le wire-agenda et la complétude du module Agenda.

## Actions

```
□ Vérifier CI status sur la PR (lint + tests + golden paths)
□ Si CI rouge → identifier le/les checks en échec → fixer dans un commit de fix
□ Si CI vert → Lead review → merge (squash)
□ task_close agenda-002 "Merged PR #XX"
□ Vérifier que main est à jour après merge
□ Renommer en done-merge-agenda-002.md
```

## Note

Si le CI échoue sur un golden path, créer une issue `bug:agenda-golden-path`
et fixer avant de merger. Ne pas merger avec des tests en échec.
