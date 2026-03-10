# todo-readme-001.md — README.md (Getting Started)

**Dépendances** : aucune
**Skills** : aucun
[MSW: non]

---

## Objectif

Écrire un `README.md` à la racine du repo qui permet à un nouveau dev
de lancer le projet localement en moins de 10 minutes.

---

## Contenu du README

### Structure

```markdown
# Vetolib — Veterinary Practice Management (UAE)

> SaaS platform for veterinary clinics in the UAE.
> Stack: .NET 10 + Next.js 15 + PostgreSQL 16 + MassTransit + MailHog

## Prerequisites

- Docker & Docker Compose
- .NET 10 SDK
- Node.js 20+
- npm 10+

## Quick Start

### 1. Clone & setup

git clone https://github.com/your-org/vetolib2
cd vetolib2

### 2. Start infrastructure

docker compose up -d
# Starts: PostgreSQL, Redis, MailHog, RabbitMQ

# Verify services:
# PostgreSQL  → localhost:5432
# MailHog UI  → http://localhost:8025  (email inbox)
# RabbitMQ UI → http://localhost:15672 (guest/guest)

### 3. Apply database migrations

dotnet ef database update --project Modules/Auth/Vetolib.Auth
dotnet ef database update --project Modules/Agenda/Vetolib.Agenda
dotnet ef database update --project Modules/MedicalRecords/Vetolib.MedicalRecords
dotnet ef database update --project Modules/Billing/Vetolib.Billing
dotnet ef database update --project Modules/Notifications/Vetolib.Notifications

### 4. Run the backend (via Aspire)

cd AppHost
dotnet run

# API available at https://localhost:7001
# Aspire dashboard → https://localhost:15888

### 5. Run the frontend

cd vetolib-frontend
npm install
npm run dev

# App → http://localhost:3000
# Default locale: EN  →  http://localhost:3000/en
# Arabic (RTL):         →  http://localhost:3000/ar

### 6. Seed data (optional)

dotnet run --project Tools/Vetolib.Seeder

# Creates:
#   Clinic: Desert Paws Veterinary (Dubai)
#   Admin: admin@desertpaws.ae / Admin@123
#   Vet: dr.sarah@desertpaws.ae / Vet@123

## Running Tests

# Unit tests
dotnet test Tests/Vetolib.Auth.Tests.Unit
dotnet test Tests/Vetolib.Agenda.Tests.Unit

# BDD / Acceptance tests (Reqnroll — requires running DB)
dotnet test Tests/Vetolib.Tests.Acceptance

# Frontend unit tests
cd vetolib-frontend && npm test

# E2E tests (Playwright — requires full stack running)
cd vetolib-frontend && npm run test:e2e

## Architecture

See archi-spec.md for the full architecture decisions.

Key points:
- Modular Monolith (Ardalis pattern) — 2 assemblies per module
- Ardalis.Result everywhere (no exceptions for business flow)
- MassTransit Outbox for all notifications (email → MailHog in dev)
- next-intl for EN/AR with full RTL support
- MSW for frontend development decoupled from backend

## Email in Development

All emails are captured by MailHog. View them at:
→ http://localhost:8025

No real emails are sent in development.

## Languages

The app supports English (EN) and Arabic (AR, RTL).
Switch language using the language selector in the top navigation.
```

## Critère de complétion

```
□ README.md à la racine du repo
□ Instructions testées : un fresh clone → projet qui tourne
□ docker-compose.yml inclut MailHog, RabbitMQ, Postgres
□ Section architecture avec lien vers archi-spec.md
□ Renommer en done-readme-001.md
```
