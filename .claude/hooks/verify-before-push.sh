#!/usr/bin/env bash
# Hook PreToolUse — bloque git push si le build ou les tests unitaires échouent
# Fichier : .claude/hooks/verify-before-push.sh
#
# Déclenché sur les commandes Bash contenant "git push".
# Exécute dotnet build + dotnet test AVANT d'autoriser le push.
# Si le build ou les tests échouent → exit 2 (bloque le push).

INPUT=$(cat)

# Extraire la commande Bash
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

echo "🔍 verify-before-push: vérification build + tests avant push..."

# Trouver la racine du repo (le hook peut être appelé depuis un worktree)
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null)
if [ -z "$REPO_ROOT" ]; then
  echo "⚠️  Impossible de trouver la racine du repo — push autorisé par défaut"
  exit 0
fi

SLN_PATH="$REPO_ROOT/src/backend/Vetolib.sln"
UNIT_TEST_PATH="$REPO_ROOT/tests/Vetolib.Tests.Unit"

# Vérifier que les chemins existent
if [ ! -f "$SLN_PATH" ]; then
  echo "⚠️  Vetolib.sln introuvable à $SLN_PATH — push autorisé par défaut"
  exit 0
fi

# Étape 1 : Build
echo "📦 Build solution..."
if ! dotnet build "$SLN_PATH" -c Release --no-restore -v quiet 2>&1 | tail -3; then
  echo ""
  echo "❌ BUILD ÉCHOUÉ — push bloqué."
  echo "Corrige les erreurs de compilation avant de push."
  exit 2
fi

# Étape 2 : Tests unitaires (rapides, pas besoin de Docker)
if [ -d "$UNIT_TEST_PATH" ]; then
  echo "🧪 Tests unitaires..."
  if ! dotnet test "$UNIT_TEST_PATH" --no-build -c Release -v quiet 2>&1 | tail -5; then
    echo ""
    echo "❌ TESTS UNITAIRES ÉCHOUÉS — push bloqué."
    echo "Corrige les tests avant de push."
    exit 2
  fi
fi

echo "✅ Build + tests OK — push autorisé."
exit 0
