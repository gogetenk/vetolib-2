#!/usr/bin/env bash
# Hook Stop — log le coût de la session quand un agent termine
# Fichier : .claude/hooks/log-cost.sh
#
# Installation dans settings.json :
# "hooks": {
#   "Stop": [{ "hooks": [{ "type": "command", "command": ".claude/hooks/log-cost.sh" }] }]
# }

INPUT=$(cat)

# Extraire les métriques de coût depuis le JSON Stop
COST=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
usage = data.get('usage', {})
cost = usage.get('total_cost_usd', 0)
input_tokens = usage.get('input_tokens', 0)
output_tokens = usage.get('output_tokens', 0)
session_id = data.get('session_id', 'unknown')[:8]
print(f'{cost:.4f}|{input_tokens}|{output_tokens}|{session_id}')
" 2>/dev/null)

if [ -z "$COST" ]; then
  exit 0
fi

TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
IFS='|' read -r COST_USD INPUT_TOK OUTPUT_TOK SESSION_ID <<< "$COST"

# Identifier quel agent tourne (depuis le nom de la tâche wip active)
WIP_TASK=$(ls tasks/wip-*.md 2>/dev/null | head -1 | xargs basename 2>/dev/null || echo "unknown")

# Ajouter une ligne dans cost-log.csv
LOG_FILE=".claude/cost-log.csv"
if [ ! -f "$LOG_FILE" ]; then
  echo "timestamp,session_id,task,cost_usd,input_tokens,output_tokens" > "$LOG_FILE"
fi

echo "$TIMESTAMP,$SESSION_ID,$WIP_TASK,$COST_USD,$INPUT_TOK,$OUTPUT_TOK" >> "$LOG_FILE"

# Alerter si coût session > $2 (signe que l'agent tourne en boucle)
COST_CENTS=$(echo "$COST_USD * 100" | bc 2>/dev/null | cut -d. -f1)
if [ "${COST_CENTS:-0}" -gt 200 ]; then
  echo "⚠️  Session coûteuse détectée : \$${COST_USD} pour ${WIP_TASK}" >> progress.md
fi

exit 0
