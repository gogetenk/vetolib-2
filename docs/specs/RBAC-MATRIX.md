# Matrice RBAC — Vetolib UAE

Matrice de controle d'acces par role pour un cabinet veterinaire UAE.

## Roles

| Role | Description |
|------|-------------|
| **Admin** | Gestionnaire de clinique. Tout voir, tout faire. Gere l'equipe. |
| **Vet** | Praticien. Cree des dossiers medicaux, prescriptions. Ne gere pas la facturation avancee. |
| **Receptionist** | Secretaire. Gere RDV, accueil (check-in), facturation. Pas de dossiers medicaux. |
| **Assistant** | Stagiaire/observateur. Lecture seule sur RDV, patients, factures. Ne cree rien. |

## Policies ASP.NET Core

| Policy | Roles couverts | Usage |
|--------|---------------|-------|
| `AdminOnly` | Admin | Gestion equipe, annulation facture |
| `VetOrAdmin` | Vet, Admin | Dossiers medicaux, consultation IN_PROGRESS/COMPLETED |
| `ClinicStaff` | Vet, Admin, Receptionist | Creation RDV, factures, items — tout sauf lecture seule |
| `VetOnly` | Vet | Prescriptions (acte medical) |
| `AdminOrReceptionist` | Admin, Receptionist | Marquer facture comme payee |

## Matrice des permissions

| Action | Admin | Vet | Receptionist | Assistant |
|--------|-------|-----|--------------|-----------|
| **Auth** | | | | |
| Login | ✓ | ✓ | ✓ | ✓ |
| Changer son mot de passe | ✓ | ✓ | ✓ | ✓ |
| Gerer les utilisateurs (inviter, role, desactiver) | ✓ | ✗ | ✗ | ✗ |
| Voir le journal d'audit | ✓ | ✗ | ✗ | ✗ |
| **Dashboard** | | | | |
| Voir les stats du tableau de bord | ✓ | ✓ | ✓ | ✓ |
| Voir les RDV du jour | ✓ | ✓ | ✓ | ✓ |
| **Agenda** | | | | |
| Creer un rendez-vous | ✓ | ✓ | ✓ | ✗ |
| Modifier un rendez-vous (reprogrammer) | ✓ | ✓ | ✓ | ✗ |
| Annuler un rendez-vous | ✓ | ✓ | ✓ | ✗ |
| Check-in patient | ✓ | ✓ | ✓ | ✗ |
| Demarrer/completer une consultation | ✓ | ✓ | ✗ | ✗ |
| Voir les rendez-vous | ✓ | ✓ | ✓ | ✓ |
| Voir les disponibilites | ✓ | ✓ | ✓ | ✓ |
| **Patients / Proprietaires** | | | | |
| Creer un patient | ✓ | ✓ | ✗ | ✗ |
| Modifier un patient | ✓ | ✓ | ✗ | ✗ |
| Voir les patients | ✓ | ✓ | ✓ | ✓ |
| Creer un proprietaire | ✓ | ✓ | ✓ | ✗ |
| **Dossiers medicaux** | | | | |
| Ajouter un dossier medical | ✓ | ✓ | ✗ | ✗ |
| Ajouter une prescription | ✗ | ✓ | ✗ | ✗ |
| Voir les dossiers medicaux | ✓ | ✓ | ✗ | ✗ |
| **Facturation** | | | | |
| Creer une facture | ✓ | ✓ | ✓ | ✗ |
| Ajouter un item a une facture | ✓ | ✓ | ✓ | ✗ |
| Envoyer une facture (DRAFT→SENT) | ✓ | ✓ | ✓ | ✗ |
| Marquer comme payee (SENT→PAID) | ✓ | ✗ | ✓ | ✗ |
| Annuler une facture | ✓ | ✗ | ✗ | ✗ |
| Voir les factures | ✓ | ✓ | ✓ | ✓ |

## Flux MustChangePassword

Lors de l'invitation d'un utilisateur :

1. `User.Invite()` cree le user avec `MustChangePassword = true`
2. Le JWT contient le claim `must_change_password: "true"` si la propriete est vraie
3. Le frontend detecte ce claim et redirige vers `/change-password`
4. `User.ChangePassword()` effectue le changement et met `MustChangePassword = false`
5. Le prochain token JWT ne contiendra plus ce claim

## Implementation backend

### Endpoints imposes par les policies

```
POST   /api/v1/appointments              → ClinicStaff
PATCH  /api/v1/appointments/{id}         → ClinicStaff
PATCH  /api/v1/appointments/{id}/status  → ClinicStaff (+ VetOrAdmin verifie en handler pour IN_PROGRESS/COMPLETED)
GET    /api/v1/appointments              → RequireAuthorization (tous)
GET    /api/v1/appointments/availability → RequireAuthorization (tous)

POST   /api/v1/invoices                  → ClinicStaff
POST   /api/v1/invoices/{id}/items       → ClinicStaff
PATCH  /api/v1/invoices/{id}/status      → ClinicStaff (+ handler verifie role par transition)
GET    /api/v1/invoices                  → RequireAuthorization (tous)

POST   /api/v1/patients/{id}/records     → VetOrAdmin (groupe)
GET    /api/v1/patients/{id}/records     → VetOrAdmin (groupe)
POST   /api/v1/patients/{id}/records/{recordId}/prescriptions → VetOnly

POST   /api/v1/patients                  → VetOrAdmin
PATCH  /api/v1/patients/{id}             → VetOrAdmin
GET    /api/v1/patients                  → RequireAuthorization (tous)

GET    /api/users                        → AdminOnly
POST   /api/users                        → AdminOnly
POST   /api/users/invite                 → AdminOnly
PATCH  /api/users/{id}/role              → AdminOnly
DELETE /api/users/{id}                   → AdminOnly
```
