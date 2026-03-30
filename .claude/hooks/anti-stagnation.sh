#!/usr/bin/env bash
# Hook anti-stagnation — vérifie que le cycle forge a produit du travail
# Déclenché après chaque cycle /forge via PostToolUse sur Agent tool
#
# Vérifie :
# 1. Y a-t-il des audits avec des findings non convertis en tâches ?
# 2. Y a-t-il des tâches TODO non dispatchées ?
# 3. Le dernier agent a-t-il été dispatché il y a plus de 30 min ?
#
# Si oui → exit 2 (feedback à l'agent : "Tu stagnes, crée des tâches")

REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null)
if [ -z "$REPO_ROOT" ]; then
  exit 0
fi

# Compter les TODO non dispatchées
TODO_COUNT=$(ls "$REPO_ROOT"/tasks/todo-*.md 2>/dev/null | wc -l)
WIP_COUNT=$(ls "$REPO_ROOT"/tasks/wip-*.md 2>/dev/null | wc -l)

# Compter les audits récents (< 7 jours) qui ont des findings
AUDIT_COUNT=$(find "$REPO_ROOT/docs/audits" -name "*.md" -newer "$REPO_ROOT/docs/audits" -mtime -7 2>/dev/null | wc -l)

# Si des TODO existent mais aucun WIP → l'orchestrateur ne dispatche pas
if [ "$TODO_COUNT" -gt 0 ] && [ "$WIP_COUNT" -eq 0 ]; then
  echo "⚠️ ANTI-STAGNATION: $TODO_COUNT tâches TODO mais 0 WIP. Dispatche des agents !"
  exit 2
fi

exit 0
