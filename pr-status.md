# pr-status.md — État des Pull Requests

> Fichier de coordination entre agents QA, PR Reviewer, PO.
> L'orchestrateur lit ce fichier pour savoir quoi lancer.

## Statuts possibles
- `[DEV_DONE]` → PR ouverte, en attente de QA et Review (parallèle)
- `[QA_FAILED]` → Tests rouges, retour au dev
- `[QA_DONE]` → Tests verts, vidéo attachée, en attente PO
- `[REVIEW_CHANGES]` → Comments PR Reviewer, en attente corrections dev
- `[REVIEW_APPROVED]` → Review technique OK
- `[PO_REJECTED]` → PO a refusé, retour au dev avec motif
- `[PO_APPROVED]` → PO a validé, prête pour merge humain
- `[MERGED]` → Mergée

---

### PR #1 — [DEV_DONE]
**Tâche** : tasks/done-back-patients-001.md
**Module** : MedicalRecords
**Branche** : feat/back-patients-001
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : en attente
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Gherkins couverts** :
- Vet creates a patient with owner inline
- Camel is a valid species for UAE market
- Patient list can be filtered by name
- Vet can update patient phone number
- Receptionist cannot create patients
- Tenant isolation on patients
- Patient detail includes medical records

**Tests unitaires** : 8 tests verts (PatientDomainTests)

---

### PR #2 — [DEV_DONE]
**Tâche** : tasks/done-wire-patients-001.md
**Module** : MedicalRecords (Patients)
**Branche** : master
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : N/A — wire annulé (Option B, cf. questions/wire-medical-001-20260309.md)
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Décision** : MSW handlers conservés pour MVP. 43 tests Playwright verts contre MSW.

---

### PR #3 — [MERGED]
**Tâche** : tasks/done-agenda-002.md
**Module** : Agenda
**Branche** : master (commit 6da39e4)
**Lien PR** : local-only (pas de remote configuré)
**Vidéo démo** : N/A
**Ouvert le** : 2026-03-09
**Dernière activité** : 2026-03-09

**Gherkins couverts** :
- Enregistrer l'arrivée du patient (check-in)
- Démarrer la consultation
- Terminer la consultation
- Annuler un rendez-vous avec motif
- Transition invalide refusée
- Consulter les disponibilités d'un vétérinaire

**Fix appliqué** : step definitions AppointmentSteps.cs alignées sur CreateAppointmentRequest v2
(PatientName/VetId/ScheduledAt) et routes `/api/appointments` — build 0 erreur.

---

## Template
### PR #{num} — [{statut}]
**Tâche** : tasks/{id}.md  
**Module** : Auth / Agenda / MedicalRecords / Billing  
**Branche** : feat/{module}-{task-id}  
**Lien PR** : {url}  
**Vidéo démo** : {url ou "en attente"}  
**Ouvert le** : {timestamp}  
**Dernière activité** : {timestamp}  
