#!/usr/bin/env bash
# Hook PreToolUse — bloque gh pr merge si le dernier CI develop est RED
# Empêche de merger sur un develop cassé

INPUT=$(cat)

COMMAND=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('command', ''))
" 2>/dev/null)

# Ne s'applique qu'aux commandes gh pr merge
if ! echo "$COMMAND" | grep -q "gh pr merge"; then
  exit 0
fi

# Vérifier que gh est disponible
if ! command -v gh &> /dev/null; then
  # Si gh n'est pas dans le PATH, essayer scoop
  export PATH="$HOME/scoop/shims:$PATH"
fi

if ! command -v gh &> /dev/null; then
  exit 0  # gh not available, skip check
fi

# Vérifier le dernier CI run sur develop
CONCLUSION=$(gh run list --branch develop --limit 1 --workflow CI --json conclusion -q '.[0].conclusion' 2>/dev/null)

if [ "$CONCLUSION" = "failure" ]; then
  echo "⚠️ GUARD-MERGE: develop CI est RED (dernier run: failure)."
  echo "Corrige le CI avant de merger d'autres PRs."
  echo "Pour bypasser: retire ce hook temporairement."
  exit 2
fi

exit 0
