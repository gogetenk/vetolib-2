# todo-back-rbac-matrix-001.md — Matrice RBAC complète + enforcement

**Module** : Auth + tous les modules
**Dépendances** : aucune
**Priorité** : BLOQUANT (pre-release)
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `aspnet-minimal-api`
**MODIF_SHARED: autorisé**

---

## Contexte

Le système actuel a 4 rôles (Admin, Vet, Receptionist, Assistant) mais :
- Assistant n'est enforced nulle part
- Agenda et Billing n'ont aucune restriction de rôle
- Pas de matrice documentée
- Pas de "force change password" pour les invités

## Matrice RBAC cible

Dans un cabinet vétérinaire UAE :

| Action | Admin | Vet | Receptionist | Assistant |
|--------|-------|-----|--------------|-----------|
| **Auth** |
| Login | ✓ | ✓ | ✓ | ✓ |
| Change own password | ✓ | ✓ | ✓ | ✓ |
| Manage users (invite, role, deactivate) | ✓ | ✗ | ✗ | ✗ |
| View audit trail | ✓ | ✗ | ✗ | ✗ |
| **Dashboard** |
| View dashboard stats | ✓ | ✓ | ✓ | ✓ |
| View today appointments | ✓ | ✓ | ✓ | ✓ |
| **Agenda** |
| Create appointment | ✓ | ✓ | ✓ | ✗ |
| Edit appointment (reschedule) | ✓ | ✓ | ✓ | ✗ |
| Cancel appointment | ✓ | ✓ | ✓ | ✗ |
| Check-in patient | ✓ | ✓ | ✓ | ✗ |
| Start/complete consultation | ✓ | ✓ | ✗ | ✗ |
| View appointments | ✓ | ✓ | ✓ | ✓ |
| View availability | ✓ | ✓ | ✓ | ✓ |
| **Patients / Owners** |
| Create patient | ✓ | ✓ | ✗ | ✗ |
| Update patient | ✓ | ✓ | ✗ | ✗ |
| View patients | ✓ | ✓ | ✓ | ✓ |
| Create owner | ✓ | ✓ | ✓ | ✗ |
| **Medical Records** |
| Add medical record | ✓ | ✓ | ✗ | ✗ |
| Add prescription | ✗ | ✓ | ✗ | ✗ |
| View medical records | ✓ | ✓ | ✗ | ✗ |
| **Billing** |
| Create invoice | ✓ | ✓ | ✓ | ✗ |
| Add invoice item | ✓ | ✓ | ✓ | ✗ |
| Send invoice (DRAFT→SENT) | ✓ | ✓ | ✓ | ✗ |
| Mark paid (SENT→PAID) | ✓ | ✗ | ✓ | ✗ |
| Cancel invoice | ✓ | ✗ | ✗ | ✗ |
| View invoices | ✓ | ✓ | ✓ | ✓ |

### Rôles expliqués

- **Admin** : Gestionnaire de clinique. Tout voir, tout faire. Gère l'équipe.
- **Vet** : Praticien. Crée des dossiers médicaux, prescriptions. Ne gère pas la facturation.
- **Receptionist** : Secrétaire. Gère RDV, accueil (check-in), facturation. Pas de dossiers médicaux.
- **Assistant** : Stagiaire/observateur. Lecture seule sur RDV, patients, factures. Ne crée rien.

## Implémentation

### 1. Nouvelles policies ASP.NET

```csharp
// Dans AuthModuleServiceRegistrar.cs ou Program.cs
options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
options.AddPolicy("VetOrAdmin", p => p.RequireRole("Vet", "Admin"));
options.AddPolicy("ClinicStaff", p => p.RequireRole("Vet", "Admin", "Receptionist"));
// "ClinicStaff" = tout sauf Assistant
// Pour lecture seule : .RequireAuthorization() (tout authentifié, y compris Assistant)
```

### 2. Appliquer sur les endpoints

**Agenda** (`AppointmentEndpoints.cs`) :
- `POST /appointments` → `ClinicStaff`
- `PATCH /appointments/{id}` → `ClinicStaff`
- `PATCH /appointments/{id}/status` → logique métier :
  - CHECK_IN : `ClinicStaff`
  - IN_PROGRESS / COMPLETED : `VetOrAdmin`
  - CANCELLED : `ClinicStaff`
- `GET /appointments` → `RequireAuthorization()` (tous)
- `GET /appointments/availability` → `RequireAuthorization()` (tous)

**Billing** (`InvoiceEndpoints.cs`) :
- `POST /invoices` → `ClinicStaff`
- `POST /invoices/{id}/items` → `ClinicStaff`
- `PATCH /invoices/{id}/status` → logique métier par transition :
  - DRAFT→SENT : `ClinicStaff`
  - SENT→PAID : `AdminOnly` ou `Receptionist` (Admin + Receptionist)
  - *→CANCELLED : `AdminOnly`
- `GET /invoices` → `RequireAuthorization()` (tous)

**Medical Records** :
- View records → `VetOrAdmin` (pas Receptionist ni Assistant — données médicales confidentielles)

### 3. Force change password on first login

- Ajouter `MustChangePassword` (bool) dans `User` entity
- Quand `InviteUserHandler` crée un user → `MustChangePassword = true`
- Ajouter claim `must_change_password` dans le JWT si true
- Frontend : rediriger vers `/change-password` si ce claim est présent
- `ChangePasswordHandler` → set `MustChangePassword = false`

### 4. Prescription = VetOnly

Les prescriptions ne doivent être ajoutées que par un Vet (pas Admin).
Le check `vetLicense` existe déjà mais renforcer avec une policy :
```csharp
options.AddPolicy("VetOnly", p => p.RequireRole("Vet"));
```
`POST /patients/{id}/records/{recordId}/prescriptions` → `VetOnly`

### 5. Documenter la matrice

Créer `docs/RBAC-MATRIX.md` avec la matrice ci-dessus.

## Critère

```
□ Policy "ClinicStaff" créée (Admin + Vet + Receptionist)
□ Policy "VetOnly" créée
□ Agenda endpoints : POST/PATCH → ClinicStaff, status transitions vérifiées
□ Billing endpoints : POST → ClinicStaff, cancel → AdminOnly
□ Medical records view → VetOrAdmin
□ Prescription → VetOnly (avec vetLicense check)
□ MustChangePassword flow implémenté (entity + claim + frontend redirect)
□ Assistant = lecture seule vérifiable
□ docs/RBAC-MATRIX.md créé
□ Tests BDD : Assistant ne peut pas créer de RDV (403)
□ Tests BDD : Receptionist ne peut pas ajouter de dossier médical (403)
□ dotnet build → 0 erreur
□ Renommer en done
```
