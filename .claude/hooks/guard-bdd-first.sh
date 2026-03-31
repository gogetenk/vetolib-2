#!/usr/bin/env bash
# Hook PreToolUse — rappelle la règle BDD-first quand un agent crée un handler sans .feature
# Se déclenche sur les Write/Edit d'un fichier Handler*.cs
# Vérifie qu'un .feature correspondant existe

INPUT=$(cat)

FILE_PATH=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('file_path') or inp.get('path') or inp.get('target_file') or '')
" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
  exit 0
fi

# Ne s'applique qu'aux fichiers Handler dans les modules
if ! echo "$FILE_PATH" | grep -qE "Handler\.cs$"; then
  exit 0
fi

# Extraire le module du chemin (ex: Modules/Agenda/... → Agenda)
MODULE=$(echo "$FILE_PATH" | grep -oP "Modules/\K[^/]+" 2>/dev/null)

if [ -z "$MODULE" ]; then
  exit 0
fi

REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null)
if [ -z "$REPO_ROOT" ]; then
  exit 0
fi

# Vérifier si des .feature existent pour ce module
FEATURE_DIR="$REPO_ROOT/tests/Vetolib.Tests.Acceptance/Features/$MODULE"
FEATURE_COUNT=$(find "$FEATURE_DIR" -name "*.feature" 2>/dev/null | wc -l)

if [ "$FEATURE_COUNT" -eq 0 ]; then
  echo "⚠️ BDD-FIRST: Vous créez un handler dans le module $MODULE mais aucun .feature n'existe dans Features/$MODULE/."
  echo "Règle 3 CLAUDE.md : 'Étape 1 : Lire les fichiers .feature liés à la tâche'."
  echo "Considérez créer les .feature AVANT d'implémenter le handler."
  # exit 0 — warning seulement, pas bloquant (certains modules n'ont pas de features intentionnellement)
fi

exit 0
