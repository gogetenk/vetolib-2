#!/usr/bin/env bash
# Hook PreToolUse — bloque toute écriture dans Shared/ sans autorisation explicite
# Fichier : .claude/hooks/guard-shared.sh
#
# Installation dans settings.json :
# "hooks": {
#   "PreToolUse": [{ "matcher": "Write|Edit|MultiEdit", "hooks": [{ "type": "command", "command": ".claude/hooks/guard-shared.sh" }] }]
# }
#
# Claude Code passe le JSON de l'appel outil sur stdin.

INPUT=$(cat)

# Extraire le chemin du fichier ciblé
FILE_PATH=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('file_path') or inp.get('path') or inp.get('target_file') or '')
" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
  exit 0  # pas de chemin détecté → laisser passer
fi

# Bloquer les modifications de Shared/ sauf si le fichier task contient "MODIF_SHARED: autorisé"
if echo "$FILE_PATH" | grep -qE "^Shared/|/Shared/"; then
  # Vérifier si une tâche wip active autorise la modification
  AUTHORIZED=$(grep -rl "MODIF_SHARED: autorisé" tasks/wip-*.md 2>/dev/null | wc -l)
  if [ "$AUTHORIZED" -eq 0 ]; then
    echo "BLOQUÉ : modification de Shared/ interdite sans autorisation explicite dans la tâche wip-*.md"
    echo "Ajoute 'MODIF_SHARED: autorisé' dans ta tâche et crée questions/{task-id}-shared-change.md pour arbitrage."
    exit 2  # exit 2 = block + afficher le message à l'agent
  fi
fi

exit 0  # laisser passer
