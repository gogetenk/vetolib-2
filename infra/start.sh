#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# Copy .env.example to .env if no .env exists yet
if [ ! -f .env ]; then
  echo "[info] No .env found — copying .env.example to .env"
  cp .env.example .env
  echo "[warn] Review .env and change the default passwords before running in production."
fi

echo "[info] Building and starting Vetolib services..."
docker compose up --build -d

echo ""
echo "Vetolib is starting up. Services:"
echo "  Frontend  : http://localhost:3000"
echo "  API       : http://localhost:8080"
echo "  API health: http://localhost:8080/health"
echo ""
echo "Demo credentials:"
echo "  admin@desertpaws.ae / Admin123!"
echo "  vet@desertpaws.ae   / Vet123!"
echo ""
echo "Run 'docker compose logs -f' to follow logs."
