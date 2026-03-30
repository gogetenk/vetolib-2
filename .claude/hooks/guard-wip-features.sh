#!/usr/bin/env bash
# Hook PreToolUse — bloque git push si des .feature @wip ont des step definitions implémentées
# Un .feature @wip avec des steps = tests désactivés = fausse couverture
#
# Logique :
# 1. Trouver tous les .feature avec @wip
# 2. Pour chacun, vérifier si des step definitions existent dans StepDefinitions/
# 3. Si oui → les tests existent mais sont désactivés → bloquer

INPUT=$(cat)

COMMAND=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('command', ''))
" 2>/dev/null)

# Ne s'applique qu'aux commandes git push
if ! echo "$COMMAND" | grep -q "git push"; then
  exit 0
fi

REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null)
if [ -z "$REPO_ROOT" ]; then
  exit 0
fi

FEATURES_DIR="$REPO_ROOT/tests/Vetolib.Tests.Acceptance/Features"
STEPS_DIR="$REPO_ROOT/tests/Vetolib.Tests.Acceptance/StepDefinitions"

if [ ! -d "$FEATURES_DIR" ] || [ ! -d "$STEPS_DIR" ]; then
  exit 0
fi

VIOLATIONS=""

# Trouver les .feature avec @wip
for feature in $(grep -rl "@wip" "$FEATURES_DIR" --include="*.feature" 2>/dev/null); do
  # Extraire le nom du module du chemin (ex: Features/Breeding/Litter.feature → Breeding)
  MODULE=$(basename "$(dirname "$feature")")

  # Vérifier si des step definitions existent pour ce module
  if [ -d "$STEPS_DIR/$MODULE" ] && [ "$(ls -A "$STEPS_DIR/$MODULE" 2>/dev/null)" ]; then
    VIOLATIONS="$VIOLATIONS\n  - $feature (steps exist in StepDefinitions/$MODULE/)"
  fi
done

if [ -n "$VIOLATIONS" ]; then
  echo "⚠️ GUARD-WIP: Ces .feature sont @wip mais ont des step definitions implémentées :"
  echo -e "$VIOLATIONS"
  echo ""
  echo "Les tests existent mais sont désactivés = fausse couverture."
  echo "Retirez le @wip tag ou supprimez les step definitions."
  exit 2
fi

exit 0
