# Breeders Features Specification — UAE + France

**Version**: 1.0
**Date**: 2026-03-25
**Author**: PO Agent
**Status**: Validated by founder

---

## 1. Business Context

### 1.1 Market Positioning

Vetolib targets two primary markets with distinct breeder profiles:

**UAE Market**
- **Falconry**: Major cultural and economic activity. Falcon hospitals (e.g., Abu Dhabi Falcon Hospital) manage lineage, breeding programs, and health records. Microchipping is mandatory.
- **Arabian Horses**: Stud farms track pedigrees across generations. EAHS (Emirates Arabian Horse Society) registration.
- **Camels**: Racing camels with breeding value. Growing veterinary segment.
- **Companion animals**: Dogs (Saluki heritage breed), cats, exotics.

**France Market**
- **LOF/LOOF registrations**: Canine (LOF — Livre des Origines Francais) and feline (LOOF) pedigree books. Breeders must track lineage for official registration.
- **Equine (Haras)**: Thoroughbred, Selle Francais, Trotteur. Stud books (SIRE/IFCE). AI and embryo transfer widely used.
- **Companion animals**: High volume of dog/cat breeding, identification by microchip mandatory (since 2012).

### 1.2 Revenue Model

The Breeding module (F5-F8) is a **paid add-on**. Phase 1 features (F1-F3) are included in the base product because they are fundamental identification fields needed by all clinics, not just breeders.

### 1.3 Phasing

| Phase | Features | Timeline | Module |
|-------|----------|----------|--------|
| Phase 1 — MVP Quick Wins | F1 (Sex), F2 (Microchip), F3 (Species) | Immediate | MedicalRecords |
| Phase 1b — MVP | F4 (Weight History) | Immediate | MedicalRecords |
| Phase 2 — Breeding Add-on | F5 (Litter), F6 (Lineage) | Q3 2026 | Breeding (new) |
| Phase 2b — Breeding Add-on | F7 (Pregnancy), F8 (Heat Cycles) | Q3 2026 | Breeding (new) |
| Phase 3 — Registry Integration | Official registries (LOF, LOOF, SIRE, EAHS) | TBD | Breeding |

---

## 2. Current Model Inventory

Before adding features, here is the current state of the Patient entity:

```
Patient (internal, MedicalRecords module)
├── Id (Guid, from BaseEntity)
├── ClinicId (Guid, IMultiTenant)
├── Name (string)
├── Species (enum: Dog, Cat, Bird, Rabbit, Horse, Exotic, Camel)
├── Breed (string)
├── BirthDate (DateOnly)
├── WeightKg (decimal?, single value)
├── PatientOwners (List<PatientOwner> — many-to-many with Owner)
└── MedicalRecords (List<MedicalRecord>)
```

**What is missing for breeders:**
- No sex/gender field
- No microchip identification
- No falcon, reptile in species enum
- Weight is a single snapshot, no history
- No lineage (parent links)
- No litter/breeding tracking
- No pregnancy/gestation management
- No heat cycle tracking

---

## 3. Phase 1 — MVP Quick Wins

### F1: Patient Sex

**Business rule**: Every patient should have a sex recorded. This is fundamental for medical decisions (dosage, surgery type, reproductive assessments) and breeding.

#### Enum: Sex
```
Male
Female
NeuteredMale
SpayedFemale
Unknown
```

**Rules:**
- Default value: `Unknown` (backward-compatible for existing data)
- Can be updated (e.g., after neutering: Male -> NeuteredMale)
- Migration: all existing patients get `Unknown`

#### Impact on DTOs

**CreatePatientRequest** — add:
- `Sex? Sex` (optional, defaults to Unknown)

**UpdatePatientRequest** — add:
- `Sex? Sex` (optional, nullable = no change)

**PatientDto** — add:
- `Sex Sex`

#### Impact on Domain

**Patient entity** — add:
- `public Sex Sex { get; private set; } = Sex.Unknown;`
- `Create()` accepts optional `Sex` parameter
- `UpdateInfo()` accepts optional `Sex` parameter

#### Frontend

- Patient creation form: Sex dropdown (Male, Female, Unknown). NeuteredMale/SpayedFemale selectable only on edit.
- Patient card: Sex displayed as icon + text next to species/breed.
- Patient list: Sex column (optional, filterable).

---

### F2: Microchip Number

**Business rule**: Microchip identification is mandatory in France (since 2012) and increasingly required in UAE. The ISO 11784/11785 standard defines a 15-digit numeric format. Vetolib must store and validate this number.

#### Fields

**Patient entity** — add:
- `public string? MicrochipNumber { get; private set; }` (nullable, 15 digits)

**Validation rules:**
- Must be exactly 15 digits (regex: `^\d{15}$`)
- Optional (nullable) — not all animals are chipped
- **Unique within a clinic** (two patients in the same clinic cannot share a microchip number; different clinics can have the same number since they are independent databases logically)
- Validation is applied on create and update, not on search

**CreatePatientRequest** — add:
- `string? MicrochipNumber`

**UpdatePatientRequest** — add:
- `string? MicrochipNumber`

**PatientDto** — add:
- `string? MicrochipNumber`

#### Search by Microchip

A new search capability: find a patient by exact microchip number match within the clinic.

**Endpoint**: `GET /api/patients?microchip={number}`

This is a query parameter on the existing patient list endpoint, not a separate endpoint.

#### Frontend

- Patient creation/edit form: Microchip Number field with input mask (15 digits).
- Patient card: Microchip number displayed with a chip icon.
- Patient search: "Search by microchip" option in the search bar.

---

### F3: Expanded Species Enum

**Business rule**: The current Species enum is too limited. UAE clinics commonly treat falcons, reptiles. The founder confirmed: no cattle/sheep/goat for now (companion + equine + falcon focus).

#### Updated Enum: Species

```
Dog        (existing)
Cat        (existing)
Bird       (existing)
Rabbit     (existing)
Horse      (existing)
Exotic     (existing)
Camel      (existing)
Falcon     (NEW — UAE falconry)
Reptile    (NEW — snakes, lizards, turtles)
```

**Note**: Cattle, Sheep, Goat are explicitly excluded per founder decision. They may be added later if the product expands to livestock.

**Migration**: No data migration needed — existing values remain valid. New values are additive.

#### Frontend

- Species dropdown updated with new values.
- Species icons: add falcon icon and reptile icon.

---

### F4: Weight History

**Business rule**: The current model stores a single `WeightKg` value. For breeding animals and growing patients, vets need to track weight over time to monitor growth curves, detect weight loss (illness indicator), and track pregnancy weight gain.

#### New Entity: WeightEntry

```
WeightEntry (Value Object, owned by Patient)
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── PatientId (Guid)
├── RecordedAt (DateOnly)
├── WeightKg (decimal)
├── Note (string?, max 500 chars)
├── RecordedBy (string — vet name)
```

**Rules:**
- WeightKg must be > 0
- WeightKg must be <= 10000 (sanity check — largest whale is ~10t, but a vet clinic realistically sees max ~2000 kg for horses/camels; 10000 as absolute ceiling)
- RecordedAt defaults to today if not provided
- Adding a weight entry automatically updates `Patient.WeightKg` to the latest value
- Weight entries are immutable (append-only, like medical records)
- Ordered by RecordedAt descending for display, ascending for chart data

#### Endpoints

- `POST /api/patients/{id}/weights` — Add a weight entry
  - Request: `{ weightKg: decimal, recordedAt?: DateOnly, note?: string }`
  - Only VET and ADMIN can add
- `GET /api/patients/{id}/weights` — Get weight history
  - Returns paginated list, most recent first
  - VET, ADMIN, ASSISTANT can view
- `GET /api/patients/{id}/weights/curve` — Get chart data
  - Returns array of `{ date, weightKg }` sorted ascending
  - Used by frontend chart component

#### Frontend

- Patient detail page: "Weight" tab showing:
  - Current weight (large, prominent)
  - Weight history table (date, weight, note, recorded by)
  - Weight curve chart (line chart, x=date, y=kg)
  - "Add weight" button (VET/ADMIN only)
- Add weight dialog: weight input (decimal), date picker (defaults today), optional note.

#### Wireframe — Weight Tab

```
+--------------------------------------------------+
| Patient: Layla (Arabian Horse)         450.5 kg   |
|--------------------------------------------------|
| [Overview] [Medical Records] [Weight] [Breeding] |
|--------------------------------------------------|
|                                                    |
|  Weight Curve                                      |
|  kg                                                |
|  460 |              *                              |
|  450 |          *       *                           |
|  440 |      *                                      |
|  430 |  *                                          |
|  420 +--+--+--+--+--+--+-->                        |
|      Jan Feb Mar Apr May Jun                       |
|                                                    |
|  [+ Add Weight]                                    |
|                                                    |
|  Date       | Weight  | Note              | By     |
|  2026-03-15 | 450.5   | Post-competition  | Dr. K  |
|  2026-02-15 | 445.0   | Monthly check     | Dr. K  |
|  2026-01-15 | 435.0   | Initial           | Dr. K  |
+--------------------------------------------------+
```

---

## 4. Phase 2 — Breeding Module (Q3 2026)

### Module Architecture

The Breeding module follows the Ardalis modular monolith pattern:

```
Modules/
  Breeding/
    Vetolib.Breeding.Contracts/    ← Public: DTOs, events, enums, interfaces
    Vetolib.Breeding/              ← Internal: handlers, entities, DbContext
```

**Inter-module communication:**
- Breeding reads patient data via `IPatientReader` interface (already in MedicalRecords.Contracts)
- Breeding does NOT reference MedicalRecords runtime
- Events: `LitterRegisteredEvent`, `PregnancyRecordedEvent` (for notifications module)

**Multi-tenancy**: All breeding entities carry `ClinicId` and are filtered by `MultiTenantDbContext`.

**Authorization:**
- VET and ADMIN: full read/write
- ASSISTANT: read-only on breeding data
- RECEPTIONIST: no access to breeding data (same as medical records)

---

### F5: Litter Management

**Business rule**: A litter represents a single birth event. It links a mother (required, must be a female Patient), an optional father (Patient in the clinic or external name), and the resulting offspring (each becomes a Patient).

#### Entity: Litter

```
Litter (AggregateRoot)
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── MotherPatientId (Guid, required)
├── FatherPatientId (Guid?, nullable — father in-clinic)
├── ExternalFatherName (string?, nullable — father not in system)
├── BirthDate (DateOnly)
├── BornCount (int)
├── AliveCount (int)
├── Notes (string?)
├── Offspring (List<LitterOffspring>)
├── CreatedAt (DateTime)
```

#### Entity: LitterOffspring (join entity)

```
LitterOffspring
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── LitterId (Guid)
├── PatientId (Guid) — the offspring as a Patient
├── BirthOrder (int?, optional)
```

#### Validation Rules

| Rule | Error message |
|------|---------------|
| Mother must exist and be Female or SpayedFemale (historical litters allowed) | "Only female patients can be registered as mothers" |
| Father and mother must be same species (if father is a Patient) | "Father and mother must be the same species" |
| AliveCount <= BornCount | "Alive count cannot exceed born count" |
| BornCount >= 1 | "At least one offspring must be born" |
| BirthDate not in the future | "Birth date cannot be in the future" |
| Either FatherPatientId or ExternalFatherName can be set, not both | "Specify either an in-clinic father or an external father name" |

#### DTOs

```csharp
// Contracts
public record CreateLitterRequest(
    Guid MotherPatientId,
    Guid? FatherPatientId,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes);

public record AddOffspringToLitterRequest(
    string Name,
    Sex Sex,
    string Breed,
    string? MicrochipNumber);

public record LitterDto(
    Guid Id,
    Guid MotherPatientId,
    string MotherName,
    Guid? FatherPatientId,
    string? FatherName,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes,
    IReadOnlyList<LitterOffspringDto> Offspring);

public record LitterOffspringDto(
    Guid PatientId,
    string Name,
    Sex Sex,
    int? BirthOrder);
```

#### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/litters` | Register a new litter |
| GET | `/api/litters/{id}` | Get litter details |
| GET | `/api/patients/{id}/litters` | List litters for a mother |
| POST | `/api/litters/{id}/offspring` | Add an offspring to a litter (creates Patient + links) |

#### Frontend — Litter View

```
+--------------------------------------------------+
| Litter — Born 2026-03-10                          |
|--------------------------------------------------|
| Mother: Shams (Arabian Horse)                     |
| Father: Buraq (Arabian Horse)                     |
| Born: 1  |  Alive: 1                              |
|--------------------------------------------------|
| Offspring                                          |
|  # | Name  | Sex  | Microchip       | Status     |
|  1 | Najm  | Male | 900118000789012 | Healthy    |
|                                                    |
| [+ Add Offspring]                                  |
|--------------------------------------------------|
| Notes: Uncomplicated delivery, foal nursing well   |
+--------------------------------------------------+
```

---

### F6: Lineage / Pedigree

**Business rule**: Any patient can have a `MotherPatientId` and `FatherPatientId` linking to other patients in the same clinic. This enables pedigree tree navigation. Lineage can be set directly (for existing patients) or automatically when adding offspring to a litter.

#### Fields on Patient

The following fields are added to the Patient entity (in MedicalRecords module, but set via Breeding module through domain events or a shared contract):

**Decision**: Since Breeding cannot modify MedicalRecords entities directly, lineage parent IDs are stored in the Breeding module's own `PatientLineage` entity.

#### Entity: PatientLineage

```
PatientLineage
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── PatientId (Guid, unique per clinic)
├── MotherPatientId (Guid?)
├── FatherPatientId (Guid?)
├── RegistryNumber (string?) — LOF, LOOF, EAHS, SIRE number
├── RegistryType (enum?: LOF, LOOF, SIRE, EAHS, FEI, Other)
```

**Note on RegistryNumber/RegistryType**: These are simple text fields in Phase 2. No validation against actual registries. Phase 3 will add API integration with official registries.

#### Validation Rules

| Rule | Error message |
|------|---------------|
| Parent and offspring must be same species | "Parent and offspring must be the same species" |
| Mother must be Female/SpayedFemale (if set) | "Mother must be a female patient" |
| Father must be Male/NeuteredMale (if set) | "Father must be a male patient" |
| Cannot set self as parent | "A patient cannot be its own parent" |
| No circular lineage (A parent of B, B parent of A) | "Circular lineage detected" |

#### Pedigree Tree

The pedigree tree is navigated up to 3 generations (configurable, default 3). Beyond 3 is rarely useful and expensive to query.

**Query approach**: Recursive CTE in PostgreSQL for ancestor navigation, or materialized in application layer for small trees.

#### DTOs

```csharp
public record SetLineageRequest(
    Guid? MotherPatientId,
    Guid? FatherPatientId,
    string? RegistryNumber,
    RegistryType? RegistryType);

public record PatientLineageDto(
    Guid PatientId,
    string PatientName,
    Guid? MotherPatientId,
    string? MotherName,
    Guid? FatherPatientId,
    string? FatherName,
    string? RegistryNumber,
    RegistryType? RegistryType);

public record PedigreeNodeDto(
    Guid PatientId,
    string Name,
    Species Species,
    string Breed,
    Sex Sex,
    string? RegistryNumber,
    PedigreeNodeDto? Mother,
    PedigreeNodeDto? Father);
```

#### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| PUT | `/api/patients/{id}/lineage` | Set/update parent links |
| GET | `/api/patients/{id}/lineage` | Get direct parents |
| GET | `/api/patients/{id}/pedigree?generations=3` | Get full pedigree tree |
| GET | `/api/patients/{id}/descendants` | Get descendants (children, grandchildren) |

#### Frontend — Pedigree Tree

```
+--------------------------------------------------+
| Pedigree — Etoile (Selle Francais)                |
|--------------------------------------------------|
|                                                    |
|                    +---------+                     |
|                    | Etoile  |                     |
|                    | SF, F   |                     |
|                    +----+----+                     |
|               __________|__________                |
|              |                     |               |
|        +-----+-----+        +-----+-----+         |
|        |  Aurore    |        |  Tonnerre  |        |
|        |  SF, F     |        |  SF, M     |        |
|        +-----+------+       +-----+------+        |
|         _____|_____           _____|_____          |
|        |           |         |           |         |
|    +---+---+  +----+---+ +--+----+ +----+---+     |
|    | Comete |  | Eclat  | | Brume | | Orage  |     |
|    | SF, F  |  | SF, M  | | SF, F | | SF, M  |     |
|    +--------+  +--------+ +-------+ +--------+     |
|                                                    |
| Registry: LOF #123456                              |
+--------------------------------------------------+
```

---

### F7: Pregnancy / Gestation Tracking

**Business rule**: Track pregnancies from mating to delivery. This is critical for equine breeding (340-day gestation), canine (63 days), feline (65 days), and falcon (31-33 days). Vets need to schedule follow-up exams and ultrasounds at appropriate intervals.

#### Entity: Pregnancy

```
Pregnancy (AggregateRoot)
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── PatientId (Guid, the mother)
├── FatherPatientId (Guid?, optional)
├── MatingDate (DateOnly)
├── MatingMethod (enum: Natural, ArtificialInsemination, EmbryoTransfer)
├── ExpectedDueDate (DateOnly, auto-calculated)
├── ActualDeliveryDate (DateOnly?)
├── Outcome (enum?: LiveBirth, Stillbirth, Miscarriage, Abortion, Unknown)
├── OffspringCount (int?)
├── Status (enum: Active, Completed, Lost)
├── Notes (string?)
├── ScheduledChecks (List<PregnancyCheck>)
├── CreatedAt (DateTime)
```

#### Entity: PregnancyCheck

```
PregnancyCheck
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── PregnancyId (Guid)
├── ScheduledDate (DateOnly)
├── CheckType (enum: Ultrasound, BloodTest, PhysicalExam, Other)
├── Note (string?)
├── CompletedAt (DateTime?)
├── Result (string?)
```

#### Species Gestation Periods (defaults)

| Species | Average Gestation (days) | Range |
|---------|-------------------------|-------|
| Dog | 63 | 58-68 |
| Cat | 65 | 60-70 |
| Horse | 340 | 320-362 |
| Camel | 390 | 360-420 |
| Falcon | 32 | 31-33 |
| Rabbit | 31 | 28-34 |

These defaults are used to auto-calculate `ExpectedDueDate` = `MatingDate + Average`.
The vet can override the expected due date manually.

#### Validation Rules

| Rule | Error message |
|------|---------------|
| Patient must be Female | "Only female patients can have pregnancies" |
| Patient must NOT be SpayedFemale | "Spayed patients cannot be pregnant" |
| MatingDate not in the future | "Mating date cannot be in the future" |
| No overlapping active pregnancy for same patient | "Patient already has an active pregnancy" |
| When completing: ActualDeliveryDate >= MatingDate | "Delivery date must be after mating date" |

#### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/pregnancies` | Record a new pregnancy |
| GET | `/api/pregnancies/{id}` | Get pregnancy details |
| GET | `/api/patients/{id}/pregnancies` | List pregnancies for a patient |
| GET | `/api/pregnancies/active` | List all active pregnancies in clinic |
| PUT | `/api/pregnancies/{id}/delivery` | Record delivery outcome |
| PUT | `/api/pregnancies/{id}/loss` | Record miscarriage/abortion |
| POST | `/api/pregnancies/{id}/checks` | Schedule a check |
| PUT | `/api/pregnancies/{id}/checks/{checkId}` | Complete a check with result |

#### Frontend — Pregnancy Timeline

```
+--------------------------------------------------+
| Pregnancy — Yasmin (Arabian Horse)                 |
|--------------------------------------------------|
| Status: Active                                     |
| Mating: 2026-02-15 (Natural)                      |
| Expected Due: 2027-01-20 (340 days)                |
| Father: Buraq (Arabian)                            |
|--------------------------------------------------|
| Timeline                                           |
|  o 2026-02-15  Mating recorded                     |
|  o 2026-03-15  Ultrasound — confirmed pregnant     |
|  * 2026-05-15  Ultrasound — scheduled              |
|  * 2026-08-15  Physical exam — scheduled            |
|  * 2027-01-20  Expected delivery                    |
|--------------------------------------------------|
| [Record Delivery] [Record Loss] [+ Schedule Check] |
+--------------------------------------------------+
```

---

### F8: Heat Cycle Tracking

**Business rule**: Recording heat cycles allows breeders to predict optimal breeding windows. This is particularly important for dogs (estrus every ~6 months), cats (seasonal polyestrus), and horses (seasonal, spring/summer).

#### Entity: HeatCycle

```
HeatCycle
├── Id (Guid)
├── ClinicId (Guid, IMultiTenant)
├── PatientId (Guid)
├── StartDate (DateOnly)
├── EndDate (DateOnly?)
├── Notes (string?)
├── CreatedAt (DateTime)
```

#### Prediction Algorithm

Based on historical data (minimum 2 cycles), calculate:
- Average interval = mean of (StartDate[n] - StartDate[n-1])
- Predicted next = last StartDate + average interval
- Confidence: displayed only if >= 3 cycles recorded

This is a simple statistical prediction, not ML. Sufficient for breeding planning.

#### Validation Rules

| Rule | Error message |
|------|---------------|
| Patient must be Female | "Only female patients can have heat cycles recorded" |
| Patient must NOT be SpayedFemale | "Spayed patients do not have heat cycles" |
| EndDate > StartDate (if EndDate provided) | "End date must be after start date" |
| No overlapping cycles for same patient | "Heat cycle overlaps with an existing cycle" |

#### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/patients/{id}/heat-cycles` | Record a heat cycle |
| GET | `/api/patients/{id}/heat-cycles` | List heat cycles for a patient |
| GET | `/api/patients/{id}/heat-cycles/prediction` | Get predicted next heat |

#### Authorization

- VET, ADMIN: full read/write
- ASSISTANT: **no access** (heat cycle data is sensitive breeding information)
- RECEPTIONIST: no access

#### Frontend — Heat Cycle View

```
+--------------------------------------------------+
| Heat Cycles — Dalma (Saluki)                       |
|--------------------------------------------------|
| Prediction: Next heat expected around 2026-07-10   |
| Average cycle: ~180 days                           |
|--------------------------------------------------|
|  Start      | End        | Duration | Notes        |
|  2026-01-10 | 2026-01-25 | 15 days  | Strong signs |
|  2025-07-10 | 2025-07-25 | 15 days  |              |
|  2025-01-15 | 2025-01-30 | 15 days  |              |
|                                                    |
| [+ Record Heat Cycle]                              |
+--------------------------------------------------+
```

---

## 5. Dependencies and Task Order

### Phase 1 — No dependencies, can be parallelized

```
F1 (Sex)        ─┐
F2 (Microchip)  ─┤── All in MedicalRecords module, independent
F3 (Species)    ─┤   Single EF migration for all three
F4 (Weight)     ─┘
```

**Recommended**: F1 + F2 + F3 in a single migration + single PR. F4 as a separate PR (new entity).

### Phase 2 — Sequential dependencies

```
F5 (Litter)   ← depends on F1 (Sex validation: mother must be Female)
F6 (Lineage)  ← depends on F5 (offspring auto-linked to parents via litter)
F7 (Pregnancy)← depends on F1 (Sex validation) + F5 (link delivery to litter)
F8 (Heat)     ← depends on F1 (Sex validation)
```

**Module creation order:**
1. Create `Vetolib.Breeding.Contracts` + `Vetolib.Breeding` assemblies
2. Register in `Vetolib.Api/Program.cs` (requires MODIF_GELE authorization)
3. Implement F5 + F6 (litter + lineage, tightly coupled)
4. Implement F7 + F8 (pregnancy + heat, can be parallelized)

---

## 6. Data Migration Notes

### Phase 1 Migration

```sql
-- F1: Add Sex column
ALTER TABLE "Patients" ADD COLUMN "Sex" integer NOT NULL DEFAULT 4; -- 4 = Unknown

-- F2: Add MicrochipNumber column
ALTER TABLE "Patients" ADD COLUMN "MicrochipNumber" varchar(15) NULL;
CREATE UNIQUE INDEX "IX_Patients_ClinicId_MicrochipNumber"
  ON "Patients" ("ClinicId", "MicrochipNumber")
  WHERE "MicrochipNumber" IS NOT NULL;

-- F3: No migration needed (enum values are additive)

-- F4: WeightEntries table
CREATE TABLE "WeightEntries" (
  "Id" uuid NOT NULL,
  "ClinicId" uuid NOT NULL,
  "PatientId" uuid NOT NULL,
  "RecordedAt" date NOT NULL,
  "WeightKg" decimal(10,2) NOT NULL,
  "Note" varchar(500) NULL,
  "RecordedBy" varchar(200) NOT NULL,
  CONSTRAINT "PK_WeightEntries" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_WeightEntries_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_WeightEntries_PatientId" ON "WeightEntries" ("PatientId");
```

### Phase 2 — New Breeding schema

Breeding entities get their own EF DbContext (`BreedingDbContext`) following the modular monolith pattern. Tables are in the same PostgreSQL database but logically isolated.

---

## 7. Gherkin Coverage Summary

| Feature File | Scenarios | Covers |
|-------------|-----------|--------|
| `PatientExtendedFields.feature` | 11 | F1 (4), F2 (5), F3 (2) |
| `WeightHistory.feature` | 8 | F4 |
| `Litter.feature` | 9 | F5 |
| `Lineage.feature` | 6 | F6 |
| `Pregnancy.feature` | 11 | F7 |
| `HeatCycle.feature` | 8 | F8 |
| **Total** | **53** | |

All feature files are tagged `@wip` and written in English per CLAUDE.md rules.

---

## 8. Open Questions for Phase 3

These are NOT blocking and are deferred per founder decision:

1. **LOF/LOOF API integration**: What is the authentication mechanism? Requires partnership agreement.
2. **SIRE/IFCE for equine**: XML-based exchange format. Need technical study.
3. **EAHS (Emirates Arabian Horse Society)**: No public API known. Manual entry may be the only option.
4. **Microchip database lookup**: Services like PetMaxx, EuroPetNet. Paid API, pricing TBD.
5. **DNA testing integration**: Growing trend in breeding. Deferred to Phase 4+.

---

## 9. Glossary

| Term | Definition |
|------|------------|
| LOF | Livre des Origines Francais — French pedigree book for dogs |
| LOOF | Livre Officiel des Origines Felines — French pedigree book for cats |
| SIRE | Systeme d'Information Relatif aux Equides — French equine registry (IFCE) |
| EAHS | Emirates Arabian Horse Society — UAE horse registry |
| ISO 11784/11785 | International standard for animal microchip identification (15-digit code) |
| AI | Artificial Insemination (in breeding context, not artificial intelligence) |
| ET | Embryo Transfer |
| Estrus | Heat period in female animals |
| Gestation | Pregnancy period from conception to birth |
| Whelping | Birth process in dogs |
| Foaling | Birth process in horses |
