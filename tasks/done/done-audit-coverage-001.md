# done-audit-coverage-001.md — Audit couverture Gherkin vs endpoints

**Module** : Cross-module
**Dépendances** : aucune
**Priorité** : MOYENNE
**Status** : DONE — 2026-03-10

---

## Résultat de l'audit

### Endpoints recensés (par module)

#### Auth — `AuthEndpoints.cs`, `UserEndpoints.cs`, `ClinicEndpoints.cs`, `OnboardingEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| POST | `/api/auth/login` | Login.feature — happy path, erreur mot de passe, lockout |
| POST | `/api/auth/refresh` | Login.feature — rotation refresh token, token révoqué/expiré |
| POST | `/api/auth/logout` | Login.feature — invalidation du refresh token |
| GET  | `/api/auth/me` | Login.feature — profil utilisateur connecté, 401 sans token |
| POST | `/api/v1/clinics/register` | ClinicSelfRegistration.feature — succès, doublon email, validation |
| POST | `/api/v1/users/` | Login.feature — admin crée user, non-admin refusé, validation |
| GET  | `/api/v1/users/` | TeamManagement.feature — liste membres |
| POST | `/api/v1/users/invite` | TeamManagement.feature — invitation email |
| PATCH | `/api/v1/users/{id}/role` | TeamManagement.feature — changement de rôle |
| DELETE | `/api/v1/users/{id}` | TeamManagement.feature — désactivation, auto-désactivation refusée |
| GET  | `/api/v1/onboarding/` | OnboardingState.feature — état initial, persistance |
| POST | `/api/v1/onboarding/steps/{stepId}/complete` | OnboardingState.feature — complétion d'étape |
| POST | `/api/v1/onboarding/banner/dismiss` | OnboardingState.feature — dismiss banner |
| POST | `/api/v1/onboarding/checklist/dismiss` | OnboardingState.feature — dismiss checklist |
| **MANQUE** | `POST /api/auth/change-password` | **AUCUN scénario** — handler `ChangePasswordCommand` implémenté mais pas exposé via endpoint |

#### Agenda — `AppointmentEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| POST | `/api/v1/appointments/` | Appointments.feature — créneau libre, conflit, hors horaires, passé |
| GET  | `/api/v1/appointments/` | Appointments.feature — lister du jour |
| PATCH | `/api/v1/appointments/{id}/status` | RBAC.feature (via rôle) mais **pas de scénario dédié happy path/400/404** |
| PATCH | `/api/v1/appointments/{id}/transition` | Appointments.feature — check-in, consultation, fin, annulation, transition invalide |
| GET  | `/api/v1/appointments/availability` | Appointments.feature — consulter les disponibilités |
| POST | `/api/v1/appointments/suggest-slot` | SlotSuggestion.feature — 7 scénarios complets |
| **MANQUE** | `GET /api/v1/appointments/{id}` | **AUCUN scénario** — query `GetAppointmentByIdQuery` existe, endpoint absent |
| **MANQUE** | `PUT /api/v1/appointments/{id}` | **AUCUN scénario** — `EditAppointmentCommand` implémenté, endpoint absent |

#### Billing — `InvoiceEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| POST | `/api/v1/invoices/` | Facturation.feature — création brouillon, TVA, numérotation |
| GET  | `/api/v1/invoices/` | Facturation.feature (implicite via scénarios enchaînés) |
| GET  | `/api/v1/invoices/{id}` | **AUCUN scénario dédié** — uniquement utilisé implicitement dans steps |
| PATCH | `/api/v1/invoices/{id}/status` | Facturation.feature — SENT, PAID, facture immuable |
| POST | `/api/v1/invoices/{id}/items` | Facturation.feature — ajout items, calcul sous-total |
| GET  | `/api/v1/invoices/{id}/pdf` | **AUCUN scénario** — endpoint implémenté, zéro coverage |

#### MedicalRecords — `PatientEndpoints.cs`, `MedicalRecordEndpoints.cs`, `OwnerEndpoints.cs`, `DrugCatalogEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| POST | `/api/v1/patients/` | Patients.feature — créer avec owner inline, espèce chameau, rôle refusé |
| GET  | `/api/v1/patients/` | Patients.feature — filtre par nom, isolation tenant |
| GET  | `/api/v1/patients/{id}` | Patients.feature (implicite) |
| GET  | `/api/v1/patients/{id}/detail` | Patients.feature — détail avec dossiers médicaux |
| PATCH | `/api/v1/patients/{id}` | Patients.feature — mise à jour numéro de téléphone |
| POST | `/api/v1/patients/import` | CsvImport.feature — succès, champs manquants, doublon email owner |
| GET  | `/api/v1/patients/import/template` | CsvImport.feature — télécharger template |
| POST | `/api/v1/patients/{id}/records` | DossierMedical.feature — ajouter examen, rôle refusé |
| GET  | `/api/v1/patients/{id}/records` | DossierMedical.feature — historique complet |
| DELETE | `/api/v1/patients/{id}/records/{recordId}` | DossierMedical.feature — "un dossier n'est jamais supprimé" |
| POST | `/api/v1/patients/{id}/records/{recordId}/prescriptions` | DossierMedical.feature, RBAC.feature, DrugInteractionChecking.feature |
| POST | `/api/v1/owners/` | **AUCUN scénario dédié** — endpoint `CreateOwner` sans scenario propre |
| GET  | `/api/v1/drugs/` | DrugInteractionChecking.feature (implicite) |
| GET  | `/api/v1/drugs/{id}` | **AUCUN scénario dédié** |
| POST | `/api/v1/drugs/` | DrugInteractionChecking.feature — "admin ajoute un médicament personnalisé" |
| POST | `/api/v1/prescriptions/preflight` | DrugInteractionChecking.feature — interactions, dosage, contraindications |

#### Dashboard — `DashboardEndpoints.cs` (Vetolib.Api host)
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| GET  | `/api/dashboard/stats` | Dashboard.feature — stats admin |
| GET  | `/api/dashboard/today-appointments` | Dashboard.feature — liste du jour |
| GET  | `/api/dashboard/recent-activity` | Dashboard.feature — feed activité récente |
| GET  | `/api/dashboard/analytics` | **AUCUN scénario** — endpoint implémenté, zéro coverage |

#### Audit — `AuditEndpoints.cs` (Vetolib.Api host)
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| GET  | `/api/audit/` | Audit.feature — admin peut consulter, non-admin refusé |

#### Stock — `StockEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| GET  | `/api/v1/stock/` | Stock.feature + StockManagement.feature — liste, filtres |
| POST | `/api/v1/stock/` | Stock.feature + StockManagement.feature — création, validation |
| PATCH | `/api/v1/stock/{id}` | StockManagement.feature — mise à jour seuil d'alerte |
| POST | `/api/v1/stock/{id}/movements` | StockManagement.feature — entrée, sortie, dépassement capacité |
| GET  | `/api/v1/stock/alerts` | Stock.feature + StockManagement.feature — alertes bas, expiration |

#### Messaging — `MessagingEndpoints.cs`, `PortalEndpoints.cs`, `SseEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| GET  | `/api/v1/messaging/conversations` | AdminMessaging + Receptionist + VetInbox features |
| GET  | `/api/v1/messaging/conversations/{id}` | VetInbox.feature — contexte médical complet |
| POST | `/api/v1/messaging/conversations/{id}/reply` | ReceptionistInbox.feature, VetInbox.feature |
| POST | `/api/v1/messaging/conversations/{id}/notes` | VetInbox.feature — note interne |
| PATCH | `/api/v1/messaging/conversations/{id}/status` | ReceptionistInbox, AdminMessaging |
| PATCH | `/api/v1/messaging/conversations/{id}/transfer` | ReceptionistInbox.feature — transfert vers vet |
| PATCH | `/api/v1/messaging/conversations/{id}/category` | MessageTriage.feature |
| POST | `/api/v1/messaging/conversations/{id}/spam` | ReceptionistInbox.feature, AdminMessaging.feature |
| POST | `/api/v1/messaging/conversations/{id}/convert-to-appointment` | ReceptionistInbox.feature, VetInbox.feature |
| POST | `/api/v1/messaging/conversations/{conversationId}/messages/{messageId}/add-to-record` | VetInbox.feature |
| POST | `/api/v1/messaging/conversations/outbound` | AdminMessaging.feature — message proactif |
| GET  | `/api/v1/messaging/conversations/{id}/summary` | VetInbox.feature — résumé IA |
| GET  | `/api/v1/messaging/settings/hours` | AdminMessaging.feature |
| PUT  | `/api/v1/messaging/settings/hours` | AdminMessaging.feature |
| GET  | `/api/v1/messaging/stats` | AdminMessaging.feature — dashboard statistiques |
| GET  | `/api/v1/messaging/templates` | AdminMessaging.feature, ReceptionistInbox.feature |
| POST | `/api/v1/messaging/templates` | AdminMessaging.feature |
| PUT  | `/api/v1/messaging/templates/{id}` | AdminMessaging.feature |
| DELETE | `/api/v1/messaging/templates/{id}` | **AUCUN scénario dédié pour DELETE template** |
| GET  | `/api/v1/portal/categories` | OwnerPortal.feature (implicite) |
| GET  | `/api/v1/portal/conversations` | OwnerPortal.feature |
| GET  | `/api/v1/portal/conversations/{id}` | OwnerPortal.feature |
| POST | `/api/v1/portal/conversations` | OwnerPortal.feature |
| POST | `/api/v1/portal/conversations/{id}/messages` | OwnerPortal.feature |
| POST | `/api/v1/portal/consent` | OwnerPortal.feature |
| GET  | `/api/v1/portal/export` | OwnerPortal.feature — download historique |
| GET  | `/api/v1/portal/pets` | OwnerPortal.feature |
| GET  | `/api/v1/messaging/sse` | **AUCUN scénario** — endpoint SSE sans coverage |

#### AI — `AIEndpoints.cs`
| Méthode | Route | Couverture Gherkin |
|---|---|---|
| POST | `/api/v1/ai/triage` | TriageVeterinaire.feature — 12 scénarios |
| PUT  | `/api/v1/ai/triage/{id}/accept` | TriageVeterinaire.feature — accepter triage |
| PUT  | `/api/v1/ai/triage/{id}/override` | TriageVeterinaire.feature — override vet |
| GET  | `/api/v1/ai/no-show-prediction/{appointmentId}` | NoShowPrediction.feature — 8 scénarios |
| POST | `/api/v1/ai/no-show-predictions/batch` | NoShowPrediction.feature — batch prédiction |
| POST | `/api/v1/ai/check-interactions` | DrugInteractionChecking.feature |

---

## Synthèse des trous critiques

| # | Endpoint manquant ou non couvert | Criticité | Module |
|---|---|---|---|
| 1 | `POST /api/auth/change-password` — handler implémenté, endpoint absent, 0 scénario | HAUTE | Auth |
| 2 | `GET /api/v1/appointments/{id}` — query implémentée, endpoint absent, 0 scénario | HAUTE | Agenda |
| 3 | `PUT /api/v1/appointments/{id}` — EditAppointmentCommand implémenté, endpoint absent, 0 scénario | HAUTE | Agenda |
| 4 | `GET /api/v1/invoices/{id}/pdf` — endpoint implémenté, 0 scénario | HAUTE | Billing |
| 5 | `GET /api/dashboard/analytics` — endpoint implémenté, 0 scénario | MOYENNE | Dashboard |
| 6 | `PATCH /api/v1/appointments/{id}/status` — pas de scénario dédié 400/404 | MOYENNE | Agenda |
| 7 | `GET /api/v1/invoices/{id}` — pas de scénario dédié (implicite seulement) | FAIBLE | Billing |
| 8 | `POST /api/v1/owners/` — endpoint sans scénario dédié | FAIBLE | MedicalRecords |
| 9 | `GET /api/v1/drugs/{id}` — endpoint sans scénario dédié | FAIBLE | MedicalRecords |
| 10 | `DELETE /api/v1/messaging/templates/{id}` — 0 scénario | FAIBLE | Messaging |
| 11 | `GET /api/v1/messaging/sse` — 0 scénario (SSE difficile à tester) | FAIBLE | Messaging |

---

## Tâches créées

- `tasks/todo-test-agenda-endpoint-gaps-001.md` — GetById + EditAppointment (endpoint + scénarios)
- `tasks/todo-test-auth-change-password-001.md` — endpoint ChangePassword + scénarios
- `tasks/todo-test-billing-pdf-001.md` — scénarios couverture PDF invoice
- `tasks/todo-test-dashboard-analytics-001.md` — scénarios couverture GET /analytics
- `tasks/todo-test-agenda-status-edge-cases-001.md` — edge cases PATCH /status (400, 404)

---

## Critères de complétion

```
[x] Liste complète des endpoints API (par module)
[x] Liste complète des scénarios Gherkin (par module)
[x] Matrice de couverture endpoint <-> scénario
[x] Tâches créées pour les trous de couverture
[ ] Rapport dans progress.md
[x] Renommer en done
```
