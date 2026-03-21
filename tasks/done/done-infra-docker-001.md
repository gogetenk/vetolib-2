# todo-infra-docker-001.md — Docker Compose pour déploiement

**Module** : Infrastructure
**Dépendances** : done-back-migrations-001
**Skills à lire** : `dotnet-aspire`

---

## Contexte

Le produit ne peut tourner que sur la machine du dev. Pour le déployer (démo client, staging, production), il faut des conteneurs Docker.

## Périmètre exact

### 1. Dockerfile backend

```dockerfile
# src/backend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Multi-stage build
# Copier sln + csproj → restore → build → publish
# ENTRYPOINT ["dotnet", "Vetolib.Api.dll"]
```

Points d'attention :
- Copier tous les `.csproj` avant le code source (cache Docker des layers restore)
- Le publish doit inclure tous les modules (ils sont chargés au runtime)
- Exposer le port 8080 (convention Aspire/Cloud Run)
- Health check endpoint : `GET /health`

### 2. Dockerfile frontend

```dockerfile
# src/frontend/Dockerfile
FROM node:20-alpine AS base
# Multi-stage : install → build → runner
# next.config.ts : output: 'standalone'
# ENTRYPOINT ["node", "server.js"]
```

Points d'attention :
- Utiliser `output: 'standalone'` dans next.config.ts pour un build léger
- Ne PAS inclure MSW dans le build production (`NODE_ENV=production`)
- Exposer le port 3000

### 3. docker-compose.yml

```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: vetolib
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-vetolib_dev}
      POSTGRES_DB: vetolibdb
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U vetolib"]

  backend:
    build:
      context: src/backend
      dockerfile: Dockerfile
    environment:
      ConnectionStrings__vetolibdb: "Host=postgres;Database=vetolibdb;Username=vetolib;Password=${POSTGRES_PASSWORD:-vetolib_dev}"
      Jwt__Secret: ${JWT_SECRET}
      ASPNETCORE_URLS: "http://+:8080"
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy

  frontend:
    build:
      context: src/frontend
      dockerfile: Dockerfile
      args:
        NEXT_PUBLIC_API_URL: http://backend:8080
    environment:
      NEXT_PUBLIC_API_URL: http://backend:8080
    ports:
      - "3000:3000"
    depends_on:
      - backend

volumes:
  pgdata:
```

### 4. .env.example

```env
POSTGRES_PASSWORD=change_me_in_production
JWT_SECRET=change_me_minimum_32_chars_long_secret_key
```

### 5. Script de démarrage rapide

Créer `scripts/start.sh` :
```bash
#!/bin/bash
docker compose up --build -d
echo "Vetolib running at http://localhost:3000"
echo "API at http://localhost:8080"
echo "Login: admin@desertpaws.ae / Admin123!"
```

### 6. .dockerignore

Pour backend et frontend, exclure node_modules, bin, obj, .git, tests, etc.

## Critère de complétion

```
□ docker compose up --build démarre les 3 services
□ PostgreSQL healthy + migrations appliquées automatiquement
□ Backend accessible sur :8080/health
□ Frontend accessible sur :3000
□ Login fonctionne end-to-end via Docker
□ .env.example documenté
□ .dockerignore présents
□ Renommer en done-infra-docker-001.md
```
