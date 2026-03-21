# Etude architecturale : multi-tenancy hybride pour le dossier medical partage

**Date** : 2026-03-10
**Auteur** : Agent architecte
**Tache** : `tasks/wip-archi-multitenant-study-001.md`
**Statut** : RECOMMANDATION TRANCHEE

---

## 1. Etat des lieux

### Architecture actuelle

Toutes les entites implementent `IMultiTenant` (propriete `ClinicId`). Le `MultiTenantDbContext` applique un global query filter `WHERE ClinicId = @current` via `Expression.Constant(ClinicContext)` sur toutes les entites qui implementent cette interface. Le `ClinicId` est extrait du JWT (claim `clinic_id`) par `ClinicContext`.

**Consequence** : un animal cree dans la clinique A est totalement invisible pour la clinique B. Un proprietaire qui change de veterinaire perd l'integralite de l'historique medical.

### Entites concernees dans MedicalRecords

| Entite | Table | IMultiTenant | Cle naturelle |
|---|---|---|---|
| Patient | `medical.patients` | Oui (ClinicId) | Nom + Species + BirthDate (pas d'identifiant biologique unique) |
| Owner | `medical.owners` | Oui (ClinicId) | Email (unique par clinique : index `ClinicId + Email`) |
| PatientOwner | `medical.patient_owners` | Oui (ClinicId) | Lien N-N Patient/Owner (unique `PatientId + OwnerId`) |
| MedicalRecord | `medical.medical_records` | Oui (ClinicId) | PatientId + ExaminedAt |
| Prescription | `medical.prescriptions` | Oui (ClinicId) | MedicalRecordId + Medication |

### Modules non concernes (restent ClinicId-scoped)

| Module | Raison |
|---|---|
| Auth (Users, Clinics, RefreshTokens) | Un utilisateur appartient a une clinique. Pas de raison de partager. |
| Agenda (Appointments) | Un RDV est physiquement dans un cabinet. Le creneaux, le veterinaire, la salle sont locaux. |
| Billing (Invoices, InvoiceItems) | La facture est emise par une entite legale (la clinique). TVA, numero de facture, comptabilite = locaux. |
| Stock | Le stock est physiquement dans un cabinet. |
| Notifications | Les notifications sont liees a des actions clinique-specifiques. |
| Messaging | Conversation entre un owner et SON cabinet. |

**Decision** : seul le module MedicalRecords est concerne par le passage en multi-tenant hybride. Les 6 autres modules restent strictement `ClinicId`-scoped. Pas de debat.

---

## 2. Recommandation architecturale

### Modele hybride : Patient global + Records avec OriginClinicId

C'est le modele recommande. Il combine l'interet produit (passeport sante animal) avec une complexite maitrisee.

#### Principe

```
Patient (GLOBAL - pas de tenant filter)
  |
  +-- PatientClinicLink (N-N : quelles cliniques ont acces a ce patient)
  |     |-- ClinicId
  |     |-- Role: ORIGIN | REFERRED | EMERGENCY
  |     |-- ConsentGrantedAt
  |     |-- ConsentRevokedAt?
  |
  +-- MedicalRecord (GLOBAL en lecture, OriginClinicId en ecriture)
  |     |-- OriginClinicId   (qui a cree ce record)
  |     |-- PatientId
  |     |-- IsLocked (true apres 72h ou quand le record est valide)
  |
  +-- Prescription (GLOBAL en lecture, OriginClinicId en ecriture)
        |-- OriginClinicId
        |-- VetLicenseNumber (tracabilite legale)

Owner (reste ClinicId-scoped)
  |
  +-- OwnerPatientLink (remplace PatientOwner, scope clinique)
```

#### Regles d'acces

| Action | Qui peut | Condition |
|---|---|---|
| Lire le dossier medical complet | Toute clinique liee au patient | PatientClinicLink existe ET ConsentRevokedAt IS NULL |
| Creer un MedicalRecord | Clinique courante | PatientClinicLink existe (auto-cree a la premiere consultation) |
| Modifier un MedicalRecord | Clinique qui l'a cree (OriginClinicId) | IsLocked = false (fenetre 72h) |
| Supprimer un MedicalRecord | Personne | Jamais. Un record medical est immutable apres verrouillage. |
| Voir les prescriptions actives | Toute clinique liee | Critique pour eviter les interactions medicamenteuses |
| Revoquer l'acces | Le proprietaire | Soft-delete du PatientClinicLink (ConsentRevokedAt = now) |

### Pourquoi pas un scope "pays" ?

Un scope pays (`CountryId` au lieu de `ClinicId`) pose plus de problemes qu'il n'en resout :

1. **Performance** : un query filter pays retourne potentiellement des millions de patients. Inutile -- une clinique ne consulte que les dossiers de ses patients actifs.
2. **Reglementation** : un animal vu aux UAE puis en France ne change pas de pays. Le dossier doit etre global, pas pays-scoped.
3. **Complexite inutile** : le modele PatientClinicLink est plus fin et couvre le cas multi-pays nativement.

**Decision** : pas de CountryId. Le Patient est global (pas de tenant filter). L'acces est controle par PatientClinicLink.

---

## 3. Impact technique detaille

### 3.1. MultiTenantDbContext (Shared/ -- GELE)

Le `MultiTenantDbContext` actuel applique le query filter sur TOUTES les entites `IMultiTenant`. Pour le modele hybride, il faut que certaines entites du module MedicalRecords ne soient PAS filtrees.

**Solution recommandee** : ne pas modifier `MultiTenantDbContext` (il est gele). A la place, le `MedicalRecordsDbContext` override le comportement pour ses entites globales.

```
Option A (recommandee) : MedicalRecordsDbContext herite DIRECTEMENT de DbContext
  - N'utilise plus MultiTenantDbContext du tout
  - Gere lui-meme le SaveChangesAsync (domain events, UpdatedAt)
  - Applique ses propres query filters :
    - Patient : pas de filter (global)
    - MedicalRecord : pas de filter (global, acces controle par PatientClinicLink)
    - PatientClinicLink : filter par ClinicId (pour que .ToListAsync() ne retourne que les liens de la clinique courante)
    - Owner : filter par ClinicId (un owner reste local)

Option B (rejetee) : ajouter une interface IGlobalEntity dans Shared.Kernel
  - Necessite de modifier Shared/ (GELE)
  - MultiTenantDbContext devrait exclure les IGlobalEntity du filter
  - Trop intrusif pour un seul module
```

**Decision** : Option A. Le `MedicalRecordsDbContext` herite directement de `DbContext` et gere lui-meme ses filtres. Duplication minime du code SaveChangesAsync (20 lignes) au benefice de l'independance totale.

**Complexite** : M (Medium)

### 3.2. Nouvelles entites

| Entite | Tenant filter | Schema |
|---|---|---|
| Patient (modifie) | AUCUN -- global | Retirer `IMultiTenant`. Ajouter un identifiant biologique optionnel (microchip number). |
| PatientClinicLink (nouveau) | ClinicId | Table de liaison patient-clinique avec consentement. |
| MedicalRecord (modifie) | AUCUN -- global | Renommer `ClinicId` en `OriginClinicId`. Retirer `IMultiTenant`. Ajouter `IsLocked`. |
| Prescription (modifie) | AUCUN -- global | Renommer `ClinicId` en `OriginClinicId`. Retirer `IMultiTenant`. |
| Owner (inchange) | ClinicId | Reste local. Un owner "Ahmed" dans la clinique A et "Ahmed" dans la clinique B sont deux entites distinctes. |

### 3.3. Identification du patient cross-clinique

Probleme : comment la clinique B sait-elle que le "Buddy" qui arrive est le meme "Buddy" que la clinique A connait ?

**Solution : microchip number (numero de puce)**

En veterinaire, la puce electronique (ISO 11784/11785) est l'identifiant universel de l'animal. Aux UAE, le pucage est obligatoire pour les chiens et chats.

```
Patient
  +-- MicrochipNumber: string? (15 digits ISO, unique globalement)
  +-- Index unique sur MicrochipNumber (WHERE NOT NULL)
```

Workflow de recherche cross-clinique :
1. La clinique B scanne la puce ou saisit le numero
2. Requete `SELECT * FROM medical.patients WHERE MicrochipNumber = @number` (sans tenant filter)
3. Si trouve : proposer de lier ce patient a la clinique B (creation PatientClinicLink)
4. Le proprietaire donne son consentement (signature numerique ou checkbox)
5. La clinique B voit tout l'historique

Pour les animaux sans puce : pas de partage possible. Le dossier reste cloisonne de fait (la clinique B cree un nouveau Patient).

**Decision** : le microchip number est le seul mecanisme de recherche cross-clinique. Pas de matching flou sur nom+espece+date (trop d'homonymes).

### 3.4. Impact sur les modules consommateurs

Les modules Agenda et Billing referencent le patient par `AnimalId` (Guid). Aujourd'hui, ce Guid est un Patient scope par ClinicId. Demain, ce Guid pointe vers un Patient global.

| Module | Propriete | Impact |
|---|---|---|
| Agenda | `Appointment.AnimalId` | Aucun changement. Le Guid reste valide. L'Appointment est toujours scope par ClinicId. |
| Billing | `Invoice.AnimalId` | Aucun changement. Meme raisonnement. |

Les deux modules stockent aussi `AnimalName` / `OwnerName` en denormalise (pattern "snapshot at creation time"). Pas d'impact.

**La communication inter-modules se fait deja par Guid, pas par requete jointe.** Le passage du Patient en global ne casse aucun contrat.

### 3.5. Migrations EF Core

La migration sera en 3 etapes ordonnees :

**Etape 1 : Ajout des nouvelles colonnes et tables (non-breaking)**
```sql
ALTER TABLE medical.patients ADD COLUMN microchip_number VARCHAR(15) NULL;
CREATE UNIQUE INDEX ix_patients_microchip ON medical.patients (microchip_number) WHERE microchip_number IS NOT NULL;

CREATE TABLE medical.patient_clinic_links (
    id UUID PRIMARY KEY,
    patient_id UUID NOT NULL REFERENCES medical.patients(id),
    clinic_id UUID NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'ORIGIN',
    consent_granted_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    consent_revoked_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (patient_id, clinic_id)
);

ALTER TABLE medical.medical_records RENAME COLUMN clinic_id TO origin_clinic_id;
ALTER TABLE medical.medical_records ADD COLUMN is_locked BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE medical.prescriptions RENAME COLUMN clinic_id TO origin_clinic_id;
```

**Etape 2 : Backfill des PatientClinicLinks**
```sql
INSERT INTO medical.patient_clinic_links (id, patient_id, clinic_id, role, consent_granted_at)
SELECT gen_random_uuid(), id, clinic_id, 'ORIGIN', NOW()
FROM medical.patients;
```

**Etape 3 : Retrait du tenant filter sur Patient (dans le code C#)**
- `Patient` n'implemente plus `IMultiTenant`
- `MedicalRecordsDbContext` change de classe parente
- La colonne `clinic_id` sur `medical.patients` est conservee comme `OriginalClinicId` (clinique de premiere inscription) mais n'est plus utilisee pour le filtrage

**Complexite migration** : L (Large) -- 3 migrations coordonnees, backfill de donnees, changement de DbContext.

---

## 4. Consentement proprietaire

### Modele de consentement

Le consentement est **opt-in par defaut a la premiere consultation**. Concretement :

1. Le proprietaire amene son animal dans une nouvelle clinique
2. La clinique scanne la puce
3. Le systeme trouve le patient existant et affiche : "Ce patient a un dossier medical existant. Souhaitez-vous autoriser cette clinique a y acceder ?"
4. Le proprietaire accepte (UI : checkbox + confirmation)
5. Un `PatientClinicLink` est cree avec `ConsentGrantedAt = now()`

### Revocation

Le proprietaire peut revoquer l'acces d'une clinique a tout moment :
- `PatientClinicLink.ConsentRevokedAt = now()`
- La clinique perd l'acces en lecture
- Les MedicalRecords crees par cette clinique (OriginClinicId) restent visibles par les AUTRES cliniques (le record appartient au patient, pas a la clinique)

### Pas de RGPD animal

En veterinaire, il n'y a pas de "droit a l'oubli" sur les donnees medicales de l'animal. Les reglementations veterinaires imposent au contraire la conservation (5 a 10 ans selon les pays). Le consentement porte uniquement sur le PARTAGE entre cliniques, pas sur l'existence des donnees.

---

## 5. Conflits d'edition

### Probleme

Deux cliniques voient le meme patient. Que se passe-t-il si elles modifient des choses en meme temps ?

### Solution : separation des preoccupations

| Donnee | Qui peut modifier | Conflit possible ? |
|---|---|---|
| Patient (nom, espece, race, date naissance) | Uniquement la clinique d'origine (OriginalClinicId) ou le proprietaire via son app | Non -- un seul editeur |
| MedicalRecord | Uniquement la clinique qui l'a cree (OriginClinicId), pendant 72h | Non -- chaque clinique cree ses propres records |
| Prescription | Uniquement la clinique qui l'a prescrite (OriginClinicId) | Non -- meme raisonnement |

**Il n'y a pas de conflit d'edition** parce que personne n'edite les donnees d'un autre. Chaque clinique AJOUTE des records au dossier partage mais ne modifie jamais les records des autres. C'est un modele append-only pour l'historique medical.

Le seul cas d'edition est la fiche signaletique du patient (poids, nom...) : elle est modifiable uniquement par la clinique d'origine. Si l'animal change definitivement de clinique, le proprietaire peut transferer le role ORIGIN via une action explicite.

---

## 6. Plan de migration

### Phase 0 : Preparation (S - Small)
- Ajouter `MicrochipNumber` au Patient (nullable, pas de breaking change)
- Ajouter la table `PatientClinicLink`
- Aucun changement de comportement : le tenant filter reste actif

### Phase 1 : Nouveau DbContext (M - Medium)
- Creer `MedicalRecordsDbContext` heritant de `DbContext` (plus de `MultiTenantDbContext`)
- Dupliquer le code SaveChangesAsync (domain events, UpdatedAt)
- Implementer les query filters custom :
  - Patient : pas de filter
  - PatientClinicLink : filter ClinicId
  - MedicalRecord : filter via sous-requete PatientClinicLink (le patient doit etre lie a la clinique courante)
  - Owner : filter ClinicId

### Phase 2 : Renommage ClinicId (M - Medium)
- Renommer `ClinicId` en `OriginClinicId` sur MedicalRecord et Prescription
- Retirer `IMultiTenant` de Patient, MedicalRecord, Prescription
- Ajouter `IsLocked` sur MedicalRecord
- Backfill des PatientClinicLinks a partir des ClinicId existants
- Migration EF Core

### Phase 3 : API de recherche cross-clinique (M - Medium)
- Endpoint : `GET /api/patients/search?microchip={number}`
- Endpoint : `POST /api/patients/{id}/link` (creer un PatientClinicLink avec consentement)
- Endpoint : `DELETE /api/patients/{id}/link` (revoquer l'acces)
- UI frontend : ecran de recherche par puce + dialogue de consentement

### Phase 4 : Verrouillage automatique (S - Small)
- Job de fond : verrouiller les MedicalRecords de plus de 72h (`IsLocked = true`)
- Ou : verrouillage a la creation du prochain record (lazy)

### Estimation totale

| Phase | Complexite | Modules touches |
|---|---|---|
| Phase 0 | S | MedicalRecords |
| Phase 1 | M | MedicalRecords (DbContext) |
| Phase 2 | M | MedicalRecords (Domain + Infra + Migration) |
| Phase 3 | M | MedicalRecords (API) + Frontend |
| Phase 4 | S | MedicalRecords |
| **Total** | **L (Large)** | **MedicalRecords uniquement** |

Aucun autre module n'est impacte. Le Shared/ n'est pas modifie. Le contrat inter-modules (Guid PatientId) ne change pas.

---

## 7. Risques identifies

| # | Risque | Probabilite | Impact | Mitigation |
|---|---|---|---|---|
| R1 | Performance des query filters custom (sous-requete PatientClinicLink) | Moyenne | Moyen | Index composite (patient_id, clinic_id) + EXPLAIN ANALYZE sur les requetes critiques |
| R2 | Backfill incorrect des PatientClinicLinks | Faible | Eleve | Migration testee sur copie de prod, rollback prepare |
| R3 | Clinique qui refuse le partage (concurrence locale) | Moyenne | Faible | Le partage est opt-in cote proprietaire. La clinique ne peut pas empecher un proprietaire de partager. |
| R4 | Animal sans puce = pas de partage | Elevee (chats non puces) | Moyen | Accepte comme limitation produit. Encourager le pucage via l'app. |
| R5 | Duplication du SaveChangesAsync (code smell) | Certaine | Faible | 20 lignes de code. Acceptable vs modifier Shared/ (gele). Documenter dans le code pourquoi. |
| R6 | EF Core : changer de classe parente sur un DbContext existant | Faible | Moyen | Tester exhaustivement avec les migrations existantes. Le schema SQL ne change pas, seul le code C# change. |
| R7 | Tests BDD existants cassent | Certaine | Moyen | Les tests actuels utilisent un ClinicId fixe. Ils continueront a fonctionner car Phase 0 cree un PatientClinicLink pour chaque patient existant. |

---

## 8. Ce qui ne change PAS

Pour etre explicite sur les limites de cette etude :

- **Auth** : inchange. Users, Clinics, RefreshTokens restent ClinicId-scoped.
- **Agenda** : inchange. Les Appointments restent ClinicId-scoped. Le `AnimalId` est un Guid opaque.
- **Billing** : inchange. Les Invoices restent ClinicId-scoped.
- **Stock** : inchange.
- **Notifications** : inchange.
- **Messaging** : inchange.
- **Shared/Kernel** : GELE. Pas de nouvelle interface.
- **Shared/Infrastructure** : GELE. `MultiTenantDbContext` n'est pas modifie.
- **AppHost/Program.cs** : GELE.
- **Vetolib.Api/Program.cs** : GELE.

---

## 9. Synthese de la recommandation

**Oui, le dossier medical partage est faisable** avec l'architecture actuelle, sans modifier Shared/.

Le modele recommande est :
1. Patient global (pas de tenant filter), identifie par microchip number
2. MedicalRecord et Prescription globaux avec `OriginClinicId` (qui a cree)
3. Acces controle par `PatientClinicLink` (consentement du proprietaire)
4. `MedicalRecordsDbContext` herite de `DbContext` (pas de `MultiTenantDbContext`)
5. Owner reste ClinicId-scoped (un owner est local a une clinique)
6. Aucun autre module n'est impacte

**Complexite totale** : Large (4-6 semaines dev pour les 4 phases)
**Risque principal** : R1 (performance) -- mitigeable par des index
**Avantage competitif** : fort. Aucun SaaS veterinaire ne propose de passeport sante animal cross-clinique.
