# Vetolib — Veterinary Practice Management (UAE)

> SaaS platform for veterinary clinics in the UAE.
> Stack: .NET 10 + Next.js 15 + PostgreSQL 16 + MassTransit + MailHog

## Prerequisites

- Docker & Docker Compose
- .NET 10 SDK
- Node.js 20+
- npm 10+

## Configuration

Vetolib never stores secrets in `appsettings.json`. The application fails fast at startup if a required secret is missing.

### Required secrets

| Key | Description | Min length |
|---|---|---|
| `Jwt__Key` | HMAC-SHA256 signing key for JWT tokens | 32 chars |
| `ConnectionStrings__vetolibdb` | PostgreSQL connection string | — |

### Local development — .NET User Secrets

```bash
cd src/backend
dotnet user-secrets set "Jwt:Key" "dev-secret-minimum-32-characters-long!!" --project Vetolib.Api
dotnet user-secrets set "ConnectionStrings:vetolibdb" "Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres" --project Vetolib.Api
```

User Secrets are stored outside the repository (`~/.microsoft/usersecrets/` on Linux/macOS, `%APPDATA%\Microsoft\UserSecrets\` on Windows) and are never committed.

### docker compose — `.env` file

Create a `.env` file at the repo root (it is in `.gitignore`):

```dotenv
POSTGRES_PASSWORD=your_strong_password_here
JWT_SECRET=your-strong-jwt-secret-minimum-32-chars
```

Then run:

```bash
docker compose up -d
```

### Production

Inject secrets via environment variables or your secrets manager (e.g. Azure Key Vault, AWS Secrets Manager):

```
Jwt__Key=<min 32 chars, randomly generated>
ConnectionStrings__vetolibdb=Host=...;Database=vetolibdb;Username=...;Password=...
```

---

## Quick Start

### 1. Clone & setup

```bash
git clone https://github.com/your-org/vetolib2
cd vetolib2
```

### 2. Start infrastructure

```bash
docker compose up -d
# Starts: PostgreSQL, Redis, MailHog, RabbitMQ

# Verify services:
# PostgreSQL  → localhost:5432
# MailHog UI  → http://localhost:8025  (email inbox)
# RabbitMQ UI → http://localhost:15672 (guest/guest)
```

### 3. Apply database migrations

```bash
cd src/backend
dotnet ef database update --project Modules/Auth/Vetolib.Auth --startup-project ../Vetolib.Api
dotnet ef database update --project Modules/Agenda/Vetolib.Agenda --startup-project ../Vetolib.Api
dotnet ef database update --project Modules/MedicalRecords/Vetolib.MedicalRecords --startup-project ../Vetolib.Api
dotnet ef database update --project Modules/Billing/Vetolib.Billing --startup-project ../Vetolib.Api
```

### 4. Run the backend (via Aspire)

```bash
cd src/backend/AppHost
dotnet run

# API available at https://localhost:7001
# Aspire dashboard → https://localhost:15888
```

### 5. Run the frontend

```bash
cd src/frontend
npm install
npm run dev

# App → http://localhost:3000
```

### 6. Seed data (optional)

The AppHost creates a clinic and default users automatically on first run via Aspire resource configuration.

Credentials after seeding:
- Admin: `admin@desertpaws.ae` / `Admin@123`
- Vet: `dr.sarah@desertpaws.ae` / `Vet@123`

## Running Tests

```bash
# Unit tests (from repo root)
dotnet test tests/Vetolib.Tests.Unit

# BDD / Acceptance tests (Reqnroll — requires running DB)
dotnet test tests/Vetolib.Tests.Acceptance

# Frontend E2E tests (Playwright — requires full stack running)
cd src/frontend && npm run test:e2e
```

## Architecture

See [archi-spec.md](archi-spec.md) for the full architecture decisions.

Key points:

- **Modular Monolith** (Ardalis pattern) — 2 assemblies per module (`.Contracts` public, runtime `internal`)
- **Ardalis.Result everywhere** — no exceptions for business flow, `Result<T>` from Domain to Endpoint
- **Multi-tenancy** — global EF Core query filter on `ClinicId`, enforced by `MultiTenantDbContext`
- **MassTransit Outbox** for all notifications (email captured by MailHog in dev)
- **MSW (Mock Service Worker)** for frontend development, fully decoupled from backend
- **next-intl** for EN/AR with full RTL support

### Solution layout

```
src/
├── backend/
│   ├── AppHost/                   ← .NET Aspire orchestration
│   ├── ServiceDefaults/           ← OpenTelemetry, health checks
│   ├── Vetolib.Api/               ← ASP.NET Core 10 host (Minimal APIs only)
│   ├── Modules/
│   │   ├── Auth/
│   │   ├── Agenda/
│   │   ├── MedicalRecords/
│   │   └── Billing/
│   └── Shared/                    ← Frozen — BaseEntity, IMultiTenant, MultiTenantDbContext
└── frontend/                      ← Next.js 15 App Router
tests/
├── Vetolib.Tests.Acceptance/      ← Reqnroll + Testcontainers
└── Vetolib.Tests.Unit/            ← xUnit + NSubstitute
```

## Email in Development

All emails are captured by MailHog. View them at:
http://localhost:8025

No real emails are sent in development.

## Languages

The app supports English (EN) and Arabic (AR, RTL).
Switch language using the language selector in the top navigation.
