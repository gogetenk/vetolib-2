#!/usr/bin/env bash
# Hook PreToolUse — bloque l'ecriture de .feature contenant du jargon technique
# Fichier : .claude/hooks/guard-feature.sh
#
# Regle : les .feature sont la propriete du PO. Purement fonctionnels.
# Zero HTTP codes, zero URLs, zero JWT, zero database.
# La technique va dans les step definitions (.cs), pas dans les .feature.

INPUT=$(cat)

# Extraire le chemin du fichier cible
FILE_PATH=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('file_path') or inp.get('path') or inp.get('target_file') or '')
" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
  exit 0
fi

# Ne verifier que les fichiers .feature
if ! echo "$FILE_PATH" | grep -qE '\.feature$'; then
  exit 0
fi

# Extraire le contenu qui sera ecrit
CONTENT=$(echo "$INPUT" | python3 -c "
import json, sys
data = json.load(sys.stdin)
inp = data.get('tool_input', {})
print(inp.get('content') or inp.get('new_string') or '')
" 2>/dev/null)

if [ -z "$CONTENT" ]; then
  exit 0
fi

VIOLATIONS=""

# Verifier les HTTP status codes dans les steps (pas dans les donnees metier)
# On cherche les patterns "status CODE" ou "returns CODE" ou "receive.*CODE" ou "CODE error" ou "CODE Forbidden"
if echo "$CONTENT" | grep -qEi '(status|returns?|receive|code)\s+(200|201|400|401|403|404|409|422|500)|(200|201|400|401|403|404|409|422|500)\s+(error|Forbidden|Unauthorized|Not Found|Conflict|OK|Created)'; then
  VIOLATIONS="$VIOLATIONS\n- HTTP status codes detected (200, 401, 403, etc.) — use functional language instead"
fi

# Verifier les chemins API
if echo "$CONTENT" | grep -qE '(GET|POST|PUT|DELETE|PATCH)\s+/|/api/'; then
  VIOLATIONS="$VIOLATIONS\n- API paths detected (/api/, POST /, GET /) — describe the action, not the endpoint"
fi

# Verifier les termes techniques interdits (case insensitive, mots entiers)
if echo "$CONTENT" | grep -qEi '\b(JWT|JSON|SQL|ClinicId|endpoint|header[s]?|HTTP|tenant)\b'; then
  VIOLATIONS="$VIOLATIONS\n- Technical terms detected (JWT, JSON, SQL, endpoint, etc.) — use business language"
fi

# Verifier "token" sauf dans un contexte metier (e.g. "token" seul est technique)
if echo "$CONTENT" | grep -qEi '\b(access token|refresh token|authentication token|bearer token)\b'; then
  VIOLATIONS="$VIOLATIONS\n- Token references detected — describe the auth flow functionally (e.g. 'the user is authenticated')"
fi

# Verifier "database"
if echo "$CONTENT" | grep -qEi '\bin the database\b|\bfrom the database\b'; then
  VIOLATIONS="$VIOLATIONS\n- Database references detected — describe the business outcome, not the storage"
fi

# Verifier "validation error for"
if echo "$CONTENT" | grep -qEi 'validation error for'; then
  VIOLATIONS="$VIOLATIONS\n- Internal field names in validation errors — describe what the user sees, not the field name"
fi

if [ -n "$VIOLATIONS" ]; then
  echo "BLOQUE : le fichier .feature contient du jargon technique interdit."
  echo ""
  echo "Violations trouvees :"
  echo -e "$VIOLATIONS"
  echo ""
  echo "Les .feature sont purement fonctionnels (langage naturel du PO)."
  echo "La technique (HTTP codes, URLs, tokens) va dans les step definitions (.cs)."
  echo ""
  echo "Exemples de corrections :"
  echo "  'the response status is 200'  →  'the operation succeeds'"
  echo "  'I POST /api/v1/auth/login'   →  'the user logs in'"
  echo "  'I receive a JWT token'       →  'the user is authenticated'"
  echo "  'returns 403'                 →  'the user is denied access'"
  echo "  'exists in the database'      →  'the record is saved'"
  exit 2
fi

exit 0
