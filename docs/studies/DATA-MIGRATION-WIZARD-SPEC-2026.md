# Data Migration Wizard -- Product Specification

**Version**: 1.0
**Date**: 2026-03-21
**Author**: Product Design
**Status**: Draft -- Pending PO Validation
**Module**: Vetolib.Migration (new module)

---

## Executive Summary

Data lock-in is the #1 barrier to veterinary software switching. Vets accumulate years of medical records, client histories, and financial data. Competitors charge $500--$3,000 for data export, and import is often manual, risky, and incomplete.

Vetolib turns this pain into a competitive advantage:

> **"Switch to Vetolib -- we migrate your data for free."**

This spec defines a Data Migration Wizard that makes switching to Vetolib frictionless, safe, and free for all plans (self-service for Starter, white-glove assisted for Pro/Enterprise).

### Market Context -- UAE

The UAE veterinary software market is valued at $4.6M (2024), growing at 14.1% CAGR to $10M by 2030. Key competitors in the region include ezyVet, Provet Cloud, Cornerstone (IDEXX), and various local solutions. The migration wizard directly addresses the switching cost barrier in this fast-growing market.

---

## 1. Competitor Export Formats -- What We Must Import

### 1.1 Primary Targets (Day 1)

| Source Software | Region | Export Format | Notes |
|---|---|---|---|
| **ezyVet** | Global / UAE | CSV, Excel | Records dashboard export, per-entity CSV. Has API for bulk extraction. |
| **IDEXX Cornerstone** | US / UAE | Proprietary DB + CSV | On-premise SQL database. Export via IDEXX conversion service or direct DB dump. |
| **Covetrus AviMark** | US / UAE | CSV (via pimsdata.io conversion) | Legacy on-premise. Requires DB backup extraction to CSV. |
| **Provet Cloud** | EU / UAE | CSV, API | Cloud-based, API-first. CSV export available for most entities. |
| **Vetup** | France | CSV | Supports export of client files via "Advanced Search" CSV export. Imports from Vetocom, Bourgelat, Gmvet, DrVeto, Vetopartner, Assistovet. |
| **DaySmart Vet (Vx)** | US | CSV, Excel | Standard tabular exports. |

### 1.2 Secondary Targets (Post-MVP)

| Source Software | Region | Export Format |
|---|---|---|
| Shepherd | US | Limited CSV (no invoices/reports) |
| Digitail | US / EU | API + CSV |
| Animana | EU | CSV |
| Pulse (Covetrus) | US | Proprietary DB conversion |
| Vetowin | France | Proprietary |
| Bourgelat / Gmvet | France | Proprietary |

### 1.3 Generic Formats (Always Supported)

| Format | Extension | Use Case |
|---|---|---|
| CSV | `.csv` | Universal tabular data. Delimiter auto-detection (comma, semicolon, tab). |
| Excel | `.xlsx`, `.xls` | Most common for small clinics doing manual export. |
| XML | `.xml` | Some legacy systems export structured XML. |
| JSON | `.json` | Modern systems, API dumps. |
| SQL Dump | `.sql` | Direct database exports from on-premise systems. |
| ZIP Archive | `.zip` | Bundled multi-file exports (common for full clinic dumps). |

---

## 2. Data Entities to Migrate

### 2.1 Entity Priority Matrix

| Entity | Priority | Complexity | Notes |
|---|---|---|---|
| **Owners (Clients)** | P0 -- Critical | Low | Name, contact, address, preferred language. Deduplicate by phone/email. |
| **Patients (Animals)** | P0 -- Critical | Medium | Species, breed, sex, DOB, weight, microchip, linked owner. |
| **Medical Records** | P0 -- Critical | High | Consultation notes, SOAP notes, diagnoses, attachments (PDFs, images). |
| **Vaccinations** | P0 -- Critical | Medium | Vaccine name, date administered, batch, next due date. |
| **Prescriptions** | P1 -- High | Medium | Drug, dosage, frequency, duration, prescribing vet. |
| **Invoices** | P1 -- High | High | Line items, tax, payments, outstanding balances. Read-only archive. |
| **Appointments** | P2 -- Medium | Low | Future appointments only. Historical for analytics. |
| **Inventory / Stock** | P2 -- Medium | Medium | Products, quantities, pricing. |
| **Documents / Attachments** | P2 -- Medium | High | Lab results PDFs, X-rays, consent forms. File storage migration. |
| **Staff / Users** | P3 -- Low | Low | Vet names for record attribution. No credential migration. |

### 2.2 Field Mapping per Entity

#### Owners

| Vetolib Field | Common Source Fields | Mapping Notes |
|---|---|---|
| `FirstName` | first_name, prenom, fname, given_name | |
| `LastName` | last_name, nom, lname, family_name, surname | |
| `Email` | email, email_address, e-mail | Validate format |
| `Phone` | phone, telephone, mobile, cell, tel | Normalize to E.164 |
| `Address` | address, street + city + zip, adresse | Concatenate if split |
| `PreferredLanguage` | language, lang, langue | Default: `en` (UAE) |

#### Patients

| Vetolib Field | Common Source Fields | Mapping Notes |
|---|---|---|
| `Name` | patient_name, animal_name, nom_animal, pet_name | |
| `Species` | species, espece, animal_type | Map to Vetolib enum |
| `Breed` | breed, race | Fuzzy match to breed list |
| `Sex` | sex, gender, sexe | Normalize M/F/MN/FS |
| `DateOfBirth` | dob, date_of_birth, birth_date, date_naissance | Parse multiple date formats |
| `MicrochipNumber` | microchip, chip_id, id_puce | Validate 15-digit ISO |
| `Weight` | weight, poids | Convert units if needed (lb/kg) |
| `OwnerId` | owner_id, client_id, proprietaire_id | Link via mapping table |

#### Medical Records

| Vetolib Field | Common Source Fields | Mapping Notes |
|---|---|---|
| `Date` | visit_date, consultation_date, date_visite | |
| `VetName` | doctor, veterinarian, vet, praticien | Match to staff or store as text |
| `Subjective` | subjective, reason, motif, complaint | SOAP: S |
| `Objective` | objective, exam_findings, examen | SOAP: O |
| `Assessment` | assessment, diagnosis, diagnostic | SOAP: A |
| `Plan` | plan, treatment, traitement | SOAP: P |
| `Notes` | notes, comments, remarques | Fallback for non-SOAP records |

---

## 3. UI Wizard -- Step-by-Step Flow

The wizard follows a 6-step linear flow with the ability to go back at any step. Each step validates before allowing progression.

### Step 1: Source Selection

```
+--------------------------------------------------+
|  Data Migration Wizard                    Step 1/6 |
|                                                    |
|  Where is your data coming from?                   |
|                                                    |
|  [ezyVet]  [Cornerstone]  [AviMark]  [Provet]    |
|  [Vetup]   [DaySmart Vet] [Other software]        |
|                                                    |
|  Or upload generic files:                          |
|  [CSV]  [Excel]  [XML]  [JSON]  [ZIP archive]    |
|                                                    |
|  Not sure? [Request white-glove migration]         |
|                                                    |
|                              [Next ->]             |
+--------------------------------------------------+
```

**Behavior:**
- Selecting a known competitor pre-loads field mapping templates (auto-mapping).
- "Other software" shows generic upload with manual mapping.
- "Request white-glove migration" redirects to a concierge form (see Section 5).

### Step 2: File Upload

```
+--------------------------------------------------+
|  Upload your data files                    Step 2/6 |
|                                                    |
|  Source: ezyVet                                    |
|                                                    |
|  We need these files from your ezyVet export:      |
|                                                    |
|  [v] Clients.csv          12,847 rows    2.1 MB   |
|  [v] Patients.csv          8,234 rows    1.8 MB   |
|  [v] Medical Records.csv  45,612 rows   12.4 MB   |
|  [ ] Invoices.csv         (optional)               |
|  [ ] Appointments.csv     (optional)               |
|                                                    |
|  Drag & drop files here or [Browse]                |
|                                                    |
|  Max file size: 500 MB per file                    |
|  Supported: .csv .xlsx .xml .json .zip             |
|                                                    |
|                    [<- Back]  [Next ->]             |
+--------------------------------------------------+
```

**Behavior:**
- For known competitors, the wizard tells the user exactly which files to export and how (with a link to a help article showing the export steps in the source software).
- File validation on upload: encoding detection (UTF-8, Latin-1, Windows-1252), delimiter detection, row count, column headers preview.
- Large files (>50MB) trigger chunked upload with progress bar.
- ZIP archives are auto-extracted and files are matched to entity types.

### Step 3: Column Mapping

```
+--------------------------------------------------+
|  Map your columns                          Step 3/6 |
|                                                    |
|  We auto-detected most mappings. Please review:    |
|                                                    |
|  --- Clients.csv ---                               |
|  Your Column        ->  Vetolib Field    Status    |
|  "client_name"      ->  LastName          [Auto]   |
|  "first_name"       ->  FirstName         [Auto]   |
|  "email_address"    ->  Email             [Auto]   |
|  "phone_number"     ->  Phone             [Auto]   |
|  "client_since"     ->  (skip)           [Manual]  |
|  "vip_status"       ->  (skip)           [Manual]  |
|                                                    |
|  Unmapped columns: 2                               |
|  [Map "client_since" ->  ...]                      |
|  [Map "vip_status"   ->  ...]                      |
|                                                    |
|  Confidence: 94% auto-mapped                       |
|                                                    |
|                    [<- Back]  [Next ->]             |
+--------------------------------------------------+
```

**Behavior:**
- Auto-mapping uses the competitor-specific templates + fuzzy matching on column names.
- Confidence score shown (% of columns auto-mapped).
- Unmapped columns can be: (a) mapped to a Vetolib field, (b) stored in a `custom_fields` JSON blob, or (c) skipped.
- Date format auto-detection with preview: "We detected DD/MM/YYYY. Is this correct?"
- For known competitors, confidence should be >90% auto-mapped.

### Step 4: Data Preview & Validation

```
+--------------------------------------------------+
|  Review your data                          Step 4/6 |
|                                                    |
|  Summary:                                          |
|  Clients:    12,847 rows  |  12,712 valid  | 135 warnings |
|  Patients:    8,234 rows  |   8,201 valid  |  33 warnings |
|  Records:    45,612 rows  |  45,498 valid  | 114 warnings |
|                                                    |
|  --- Warnings (282 total) ---                      |
|                                                    |
|  [!] 47 duplicate clients (same email)             |
|      -> [Merge] [Keep both] [Review each]          |
|                                                    |
|  [!] 23 patients with missing species              |
|      -> [Set all to "Unknown"] [Review each]       |
|                                                    |
|  [!] 12 invalid phone numbers                      |
|      -> [Import without phone] [Review each]       |
|                                                    |
|  [!] 114 medical records with unparseable dates    |
|      -> [Use upload date] [Review each]            |
|                                                    |
|  Preview (first 10 rows):                          |
|  | Name          | Species | Owner        | ... |  |
|  | Luna          | Dog     | Ahmed K.     | ... |  |
|  | Simba         | Cat     | Sarah M.     | ... |  |
|  | Rex           | Dog     | Omar H.      | ... |  |
|                                                    |
|                    [<- Back]  [Start Import ->]    |
+--------------------------------------------------+
```

**Behavior:**
- Full validation pass runs server-side before showing this screen.
- Warnings are grouped by category with bulk actions.
- "Review each" opens a modal for row-by-row correction.
- Zero-warning imports skip this step (auto-advance).
- No data is written to the database until the user clicks "Start Import".

### Step 5: Import Progress

```
+--------------------------------------------------+
|  Importing your data...                    Step 5/6 |
|                                                    |
|  [=============================--------]  72%      |
|                                                    |
|  Clients:         12,712 / 12,712    DONE          |
|  Patients:         5,934 /  8,201    Importing...  |
|  Medical Records:      0 / 45,498    Pending       |
|  Invoices:             0 /  3,245    Pending       |
|                                                    |
|  Estimated time remaining: ~4 minutes              |
|                                                    |
|  You can close this page. We will notify you       |
|  by email when the import is complete.             |
|                                                    |
|                              [Cancel Import]       |
+--------------------------------------------------+
```

**Behavior:**
- Import runs as a background job (see Section 6).
- Real-time progress via SignalR/WebSocket.
- User can close the browser -- email notification on completion.
- Cancel triggers a full rollback (transactional import).
- Import order respects dependencies: Owners first, then Patients (linked to Owners), then Medical Records (linked to Patients), then Invoices.

### Step 6: Import Complete

```
+--------------------------------------------------+
|  Migration Complete!                       Step 6/6 |
|                                                    |
|  Your data has been successfully imported:          |
|                                                    |
|  Clients:          12,712 imported                 |
|  Patients:          8,201 imported                 |
|  Medical Records:  45,498 imported                 |
|  Invoices:          3,245 imported (read-only)     |
|                                                    |
|  47 duplicate clients were merged                  |
|  23 patients marked as "Species: Unknown"          |
|                                                    |
|  [Download full import report (PDF)]               |
|                                                    |
|  What's next?                                      |
|  -> [Explore your patients]                        |
|  -> [Set up your schedule]                         |
|  -> [Invite your team]                             |
|                                                    |
+--------------------------------------------------+
```

**Behavior:**
- Detailed PDF report with every row imported, skipped, or merged.
- Imported invoices are read-only (historical archive, not editable).
- Quick links to key areas of the app for immediate productivity.
- Migration audit log stored permanently for compliance.

---

## 4. Error Handling Strategy

### 4.1 Duplicate Detection

| Entity | Duplicate Key | Strategy |
|---|---|---|
| Owners | Email OR Phone (normalized) | Suggest merge: keep newest contact info, combine notes. |
| Patients | Name + Species + Owner | Suggest merge if same owner. Flag if different owners. |
| Medical Records | Patient + Date + VetName | Skip exact duplicates. Import near-duplicates with warning. |
| Invoices | Invoice Number | Skip if same number exists. |

### 4.2 Missing Required Fields

| Missing Field | Default Action | User Override |
|---|---|---|
| Owner email | Import with empty email | User can fill in manually |
| Patient species | Set to "Unknown" | User can map from source field |
| Record date | Use file modification date | User can set manually |
| Patient-Owner link | Create "Unlinked Patients" group | User can assign owners later |

### 4.3 Format Validation Errors

| Error Type | Handling |
|---|---|
| Invalid email format | Import without email, flag for later correction |
| Invalid phone format | Attempt normalization (remove spaces, add country code). If still invalid, import without phone. |
| Unparseable date | Try 10+ common date formats (ISO, US, EU, Arabic calendar). Last resort: prompt user for format. |
| Encoding issues | Auto-detect encoding. If garbled, try UTF-8 -> Latin-1 -> Windows-1252 fallback chain. |
| Truncated file | Detect via row count mismatch or missing EOF. Alert user to re-export. |

### 4.4 Rollback Strategy

- The entire import is wrapped in a transaction scope.
- If the user cancels mid-import or a critical error occurs, ALL imported data for this session is rolled back.
- Partial imports are never left in the database.
- After successful import, a 30-day "undo window" allows full rollback via Settings > Data Migration > Undo Last Import.

---

## 5. White-Glove Migration Service

### 5.1 Eligibility

| Plan | Migration Service | Cost |
|---|---|---|
| **Starter** | Self-service wizard only | Free |
| **Pro** | White-glove: we do it for you | Free (included in plan) |
| **Enterprise** | White-glove + dedicated migration specialist | Free (included in plan) |

### 5.2 White-Glove Process

```
Day 0:  Clinic signs up for Pro/Enterprise plan
Day 1:  Migration specialist contacts clinic
Day 2:  Specialist requests backup/export from current software
        (guides clinic through export steps via video call)
Day 3:  Specialist runs import on staging environment
Day 4:  Clinic validates data on staging
Day 5:  Specialist runs final import on production
Day 6:  Clinic goes live on Vetolib
Day 7:  Follow-up call to verify everything
```

### 5.3 White-Glove Scope

- Specialist handles all file preparation, mapping, and validation.
- For on-premise systems (Cornerstone, AviMark): specialist can remote-access the clinic's server (with permission) to extract the database backup.
- For proprietary formats: specialist uses internal conversion scripts.
- Data cleaning included: deduplication, phone normalization, species/breed standardization.
- Post-migration: specialist verifies 20 random patient records with the clinic to confirm accuracy.
- SLA: migration completed within 5 business days of receiving the data.

### 5.4 Sales Positioning

The white-glove service is a sales weapon:

- **Objection: "Migration is too risky"** -- Response: "We do it for you. You just validate."
- **Objection: "My old software won't export"** -- Response: "Our specialist can remote-access your server and extract the data directly."
- **Objection: "It costs $2,000 at competitor X"** -- Response: "It's free with Vetolib Pro. Zero cost, zero risk."
- **Objection: "I'll lose data"** -- Response: "We run on a staging copy first. You verify before anything goes live. And you can undo for 30 days."

---

## 6. Technical Architecture

### 6.1 Module Structure (Ardalis Pattern)

```
Modules/
  Migration/
    Vetolib.Migration.Contracts/     -- PUBLIC: DTOs, enums, events
      MigrationStatus.cs             -- enum: Pending, Mapping, Validating, Importing, Complete, Failed, RolledBack
      MigrationSource.cs             -- enum: EzyVet, Cornerstone, AviMark, Provet, Vetup, Generic
      MigrationSessionDto.cs
      MigrationProgressEvent.cs      -- MediatR notification for real-time progress
      IMigrationService.cs           -- interface for cross-module access

    Vetolib.Migration/               -- INTERNAL: handlers, entities, import logic
      Entities/
        MigrationSession.cs          -- aggregate root: tracks one import session
        MigrationFile.cs             -- uploaded file metadata
        MigrationMapping.cs          -- column mapping configuration
        MigrationError.cs            -- per-row error tracking
      Handlers/
        CreateMigrationSessionHandler.cs
        UploadMigrationFileHandler.cs
        AutoMapColumnsHandler.cs
        ValidateMigrationDataHandler.cs
        StartImportHandler.cs
        CancelImportHandler.cs
        RollbackImportHandler.cs
        GetMigrationProgressHandler.cs
      Services/
        FileParserService.cs         -- CSV, Excel, XML, JSON parsing
        ColumnAutoMapper.cs           -- fuzzy matching + competitor templates
        DataValidatorService.cs       -- per-entity validation rules
        DuplicateDetectorService.cs   -- deduplication logic
        BulkImportService.cs          -- EF Core bulk insert (batched)
        ProgressTracker.cs            -- SignalR hub for real-time updates
      MappingTemplates/
        ezyvet-mapping.json           -- pre-built column mappings for ezyVet
        cornerstone-mapping.json
        avimark-mapping.json
        provet-mapping.json
        vetup-mapping.json
      Infrastructure/
        MigrationDbContext.cs
        MigrationModule.cs            -- ModuleServiceRegistrar
```

### 6.2 API Endpoints (Minimal API)

```
POST   /api/v1/migration/sessions                    -- Create a new migration session
GET    /api/v1/migration/sessions/{id}                -- Get session status
DELETE /api/v1/migration/sessions/{id}                -- Cancel/delete session

POST   /api/v1/migration/sessions/{id}/files          -- Upload file(s)
DELETE /api/v1/migration/sessions/{id}/files/{fileId}  -- Remove uploaded file

GET    /api/v1/migration/sessions/{id}/mapping         -- Get auto-detected mapping
PUT    /api/v1/migration/sessions/{id}/mapping         -- Update/confirm mapping

POST   /api/v1/migration/sessions/{id}/validate        -- Run validation pass
GET    /api/v1/migration/sessions/{id}/validation       -- Get validation results

POST   /api/v1/migration/sessions/{id}/import          -- Start import (background job)
GET    /api/v1/migration/sessions/{id}/progress         -- Get import progress (also via SignalR)
POST   /api/v1/migration/sessions/{id}/cancel           -- Cancel running import
POST   /api/v1/migration/sessions/{id}/rollback         -- Rollback completed import

GET    /api/v1/migration/sessions/{id}/report           -- Download import report (PDF)
```

### 6.3 Background Job Architecture

```
Upload Phase (synchronous):
  Browser  -->  API  -->  Azure Blob Storage / local file storage
                     -->  MigrationSession created (status: Pending)

Mapping Phase (synchronous, fast):
  API  -->  FileParserService (read headers)
       -->  ColumnAutoMapper (fuzzy match)
       -->  Return mapping to UI

Validation Phase (background job):
  API  -->  Enqueue validation job
  Job  -->  FileParserService (stream all rows)
       -->  DataValidatorService (per-row validation)
       -->  DuplicateDetectorService (cross-file dedup)
       -->  Store results in MigrationError table
       -->  SignalR: push progress to UI

Import Phase (background job, transactional):
  API  -->  Enqueue import job
  Job  -->  Begin transaction
       -->  BulkImportService: Owners (batch of 500)
       -->  BulkImportService: Patients (batch of 500, linked to Owners)
       -->  BulkImportService: Medical Records (batch of 200)
       -->  BulkImportService: Vaccinations
       -->  BulkImportService: Prescriptions
       -->  BulkImportService: Invoices (read-only flag)
       -->  Commit transaction
       -->  SignalR: push completion event
       -->  Email: send completion notification
       -->  Generate PDF report
```

### 6.4 Key Technical Decisions

| Decision | Rationale |
|---|---|
| **EF Core bulk insert (batched, not raw SQL)** | Respects multi-tenancy global query filter. ClinicId auto-applied. |
| **Single transaction for entire import** | Guarantees atomicity. No partial data states. |
| **Background job (Hangfire or .NET Aspire job)** | Large imports (50K+ records) take minutes. User should not wait. |
| **SignalR for progress** | Real-time UX without polling. |
| **Mapping templates as JSON files** | Easy to add new competitors without code changes. Community can contribute. |
| **30-day rollback window** | All imported records tagged with `MigrationSessionId`. Rollback = delete by session. |
| **Files stored in blob storage** | Uploaded files kept for 90 days for re-processing if needed. |

### 6.5 Multi-Tenancy Considerations

- `MigrationSession` is a multi-tenant entity (has `ClinicId`).
- All imported data inherits the current clinic's `ClinicId` via the global query filter.
- White-glove specialists access clinics via an admin impersonation mechanism (audited).
- Migration files are stored in tenant-isolated blob containers.

---

## 7. Effort Estimation

### 7.1 Development Phases

| Phase | Scope | Effort | Dependencies |
|---|---|---|---|
| **Phase 1: Core Wizard** | Upload, generic CSV/Excel parsing, manual mapping, basic validation, import for Owners + Patients | 3 sprints (6 weeks) | MedicalRecords module complete |
| **Phase 2: Smart Mapping** | Auto-mapping engine, competitor templates (ezyVet, Cornerstone, AviMark), fuzzy matching | 2 sprints (4 weeks) | Phase 1 |
| **Phase 3: Full Entity Support** | Medical Records, Vaccinations, Prescriptions, Invoices import | 2 sprints (4 weeks) | Phase 2 |
| **Phase 4: Background Jobs & UX** | Background import, SignalR progress, email notifications, PDF report, rollback | 2 sprints (4 weeks) | Phase 3 |
| **Phase 5: White-Glove Tooling** | Admin panel for migration specialists, staging environment copy, audit logging | 1 sprint (2 weeks) | Phase 4 |
| **Phase 6: Competitor Expansion** | Provet, Vetup, DaySmart templates, proprietary format parsers | Ongoing (1 sprint per competitor) | Phase 2 |

**Total MVP (Phases 1-4): 9 sprints / 18 weeks**
**Total with White-Glove (Phases 1-5): 10 sprints / 20 weeks**

### 7.2 Team Composition

| Role | Allocation | Responsibility |
|---|---|---|
| Backend Engineer | 1 FTE | Import engine, validation, bulk insert, background jobs |
| Frontend Engineer | 1 FTE | Wizard UI, mapping interface, progress tracking |
| QA Engineer | 0.5 FTE | Test with real competitor exports, edge cases |
| Product Designer | 0.25 FTE | Wizard UX, error handling flows |
| Migration Specialist | 1 FTE (post-launch) | White-glove service delivery |

### 7.3 Key Risks

| Risk | Mitigation |
|---|---|
| Competitor changes export format | Mapping templates are external JSON. Quick to update without deploy. |
| Large imports timeout | Background jobs with checkpointing. Resume from last successful batch. |
| Data quality in source files | Comprehensive validation with user-friendly error messages. |
| Encoding/locale issues (Arabic names in UAE) | UTF-8 enforced. RTL text preserved. Arabic name normalization. |
| GDPR/data privacy (France market) | Uploaded files auto-deleted after 90 days. Audit trail for white-glove access. |

---

## 8. Success Metrics

| Metric | Target | Measurement |
|---|---|---|
| Migration completion rate | >90% of started wizards complete | Analytics on wizard funnel |
| Auto-mapping accuracy | >85% columns auto-mapped for known competitors | Mapping confirmation logs |
| Time to complete self-service migration | <30 minutes for <10K records | Session duration tracking |
| White-glove SLA compliance | 100% within 5 business days | Internal SLA dashboard |
| Post-migration support tickets | <5% of migrated clinics | Zendesk tag analysis |
| Conversion rate uplift | +15% on signup-to-paid conversion | A/B test with/without migration offer |

---

## 9. Competitive Positioning Summary

| Competitor | Migration Cost | Migration Method | Vetolib Advantage |
|---|---|---|---|
| ezyVet | High fees | Self-service CSV + paid onboarding | Free + auto-mapping for ezyVet format |
| Cornerstone (IDEXX) | $500--$3,000 | Vendor-assisted only | Free + white-glove included |
| AviMark (Covetrus) | $500+ via pimsdata.io | Third-party conversion service | Free + built-in AviMark parser |
| Shepherd | Free (limited) | No invoices, no reports migrated | Free + ALL data types included |
| Vetup | Included (manual) | Remote access + manual transfer | Free + automated wizard + validation |

**Vetolib's migration offer eliminates the #1 objection to switching veterinary software.** Combined with the self-service wizard for small clinics and white-glove service for larger practices, this feature becomes a decisive sales argument in the UAE market and beyond.

---

## Sources

- [Pims Data Migration -- AviMark to CSV conversion](https://pimsdata.io/)
- [Digitail -- How to Switch Veterinary PIMS](https://digitail.com/blog/how-to-switch-veterinary-practice-management-software-without-losing-your-mind-or-your-clients/)
- [Puppilot -- Vet Data Interoperability Guide](https://www.puppilot.co/blog/veterinary-data-interoperability-the-complete-guide-to-connecting-pims-labs-insurers)
- [ezyVet -- Records Dashboard Export](https://docs.ezyvet.com/en/browse-documentation/ezyvet/getting-started/find-and-show-information/record-search/the-records-dashboard-tab/export-search-results-of-the-records-dashboard)
- [ezyVet -- Report File Formats](https://docs.ezyvet.com/en/browse-documentation/ezyvet/getting-started/find-and-show-information/reports/report-file-formats)
- [Vetup -- Data Transfer Methods](https://help.vetup.com/methode-transfert-donnees/)
- [Vetup -- Data Import Steps](https://help.vetup.com/etapes-import-donnees/)
- [VetSyCare -- Veterinary Software Pricing Guide 2026](https://vetsycare.com/blog/veterinary-software-pricing-guide)
- [NectarVet -- Best Cloud-Based Veterinary Software](https://www.nectarvet.com/post/best-cloud-based-vet-software-prices-reviews)
- [Grand View Research -- UAE Veterinary Software Market 2030](https://www.grandviewresearch.com/horizon/outlook/veterinary-software-market/uae)
- [IDEXX Cornerstone Software](https://software.idexx.com/products/cornerstone)
- [Otto -- PIMS Integration Guide](https://otto.vet/otto-flow/the-definitive-guide-to-veterinary-pims-integration-automation/)
