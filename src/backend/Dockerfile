# ── Stage 1: restore ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy solution and all .csproj files first to maximise layer cache on restore.
COPY Vetolib.sln ./

COPY Vetolib.Api/Vetolib.Api.csproj                                                                   Vetolib.Api/
COPY ServiceDefaults/Vetolib.ServiceDefaults.csproj                                                   ServiceDefaults/
COPY Shared/Vetolib.Shared.Kernel/Vetolib.Shared.Kernel.csproj                                       Shared/Vetolib.Shared.Kernel/
COPY Shared/Vetolib.Shared.Infrastructure/Vetolib.Shared.Infrastructure.csproj                       Shared/Vetolib.Shared.Infrastructure/
COPY Modules/Auth/Vetolib.Auth.Contracts/Vetolib.Auth.Contracts.csproj                               Modules/Auth/Vetolib.Auth.Contracts/
COPY Modules/Auth/Vetolib.Auth/Vetolib.Auth.csproj                                                   Modules/Auth/Vetolib.Auth/
COPY Modules/Agenda/Vetolib.Agenda.Contracts/Vetolib.Agenda.Contracts.csproj                         Modules/Agenda/Vetolib.Agenda.Contracts/
COPY Modules/Agenda/Vetolib.Agenda/Vetolib.Agenda.csproj                                             Modules/Agenda/Vetolib.Agenda/
COPY Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/Vetolib.MedicalRecords.Contracts.csproj Modules/MedicalRecords/Vetolib.MedicalRecords.Contracts/
COPY Modules/MedicalRecords/Vetolib.MedicalRecords/Vetolib.MedicalRecords.csproj                     Modules/MedicalRecords/Vetolib.MedicalRecords/
COPY Modules/Billing/Vetolib.Billing.Contracts/Vetolib.Billing.Contracts.csproj                      Modules/Billing/Vetolib.Billing.Contracts/
COPY Modules/Billing/Vetolib.Billing/Vetolib.Billing.csproj                                          Modules/Billing/Vetolib.Billing/
COPY Modules/Notifications/Vetolib.Notifications.Contracts/Vetolib.Notifications.Contracts.csproj    Modules/Notifications/Vetolib.Notifications.Contracts/
COPY Modules/Notifications/Vetolib.Notifications/Vetolib.Notifications.csproj                        Modules/Notifications/Vetolib.Notifications/
COPY Modules/AI/Vetolib.AI.Contracts/Vetolib.AI.Contracts.csproj                                     Modules/AI/Vetolib.AI.Contracts/
COPY Modules/AI/Vetolib.AI/Vetolib.AI.csproj                                                         Modules/AI/Vetolib.AI/
COPY Modules/Messaging/Vetolib.Messaging.Contracts/Vetolib.Messaging.Contracts.csproj                Modules/Messaging/Vetolib.Messaging.Contracts/
COPY Modules/Messaging/Vetolib.Messaging/Vetolib.Messaging.csproj                                    Modules/Messaging/Vetolib.Messaging/
COPY Modules/Stock/Vetolib.Stock.Contracts/Vetolib.Stock.Contracts.csproj                            Modules/Stock/Vetolib.Stock.Contracts/
COPY Modules/Stock/Vetolib.Stock/Vetolib.Stock.csproj                                                Modules/Stock/Vetolib.Stock/
COPY Modules/Preferences/Vetolib.Preferences.Contracts/Vetolib.Preferences.Contracts.csproj          Modules/Preferences/Vetolib.Preferences.Contracts/
COPY Modules/Preferences/Vetolib.Preferences/Vetolib.Preferences.csproj                              Modules/Preferences/Vetolib.Preferences/

# AppHost is Aspire-only — not needed for Docker deployment; exclude it.
# The solution references it, so we add a stub entry to satisfy the sln without building it.

RUN dotnet restore Vetolib.Api/Vetolib.Api.csproj

# ── Stage 2: build ───────────────────────────────────────────────────────────
FROM restore AS build
WORKDIR /src

COPY . .

RUN dotnet publish Vetolib.Api/Vetolib.Api.csproj \
    --no-restore \
    -c Release \
    -o /app/publish

# ── Stage 3: runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Vetolib.Api.dll"]
