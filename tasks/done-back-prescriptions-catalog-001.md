# todo-back-prescriptions-catalog-001 -- DrugCatalogEntry entity + seed data

**Module** : MedicalRecords
**Phase** : 1 (Drug Catalog + Prescription Linking)
**Dependances** : aucune
**Branchement ulterieur** : todo-front-prescriptions-catalog-001

## Objectif

Creer l'entite `DrugCatalogEntry` dans le module MedicalRecords avec les sous-entites (contraindications, interactions, dosage guidelines) et seeder ~500 medicaments veterinaires.

## Skills a lire

1. `skills/ardalis-result/SKILL.md`
2. `skills/cqrs-mediatr/SKILL.md`
3. `skills/ardalis-modular-monolith/SKILL.md`
4. `skills/multitenant-efcore/SKILL.md`

## Scope detaille

### Contracts (Vetolib.MedicalRecords.Contracts)

- `DrugCatalogEntryDto` : record avec Id, InnName, DisplayName, Category, SpeciesContraindications, DosageGuidelines
- `DrugCategory` : enum (Medication, Vaccine, Supplement)
- `InteractionSeverity` : enum (Critical, Moderate, Info)
- `InteractionAlertType` : enum (SpeciesContraindication, DrugInteraction, DosageOutOfRange)
- `SpeciesContraindicationDto` : record (Species, Severity, Reason)
- `DrugInteractionDto` : record (OtherDrugId, OtherDrugName, Severity, Description)
- `DosageGuidelineDto` : record (Species, MinDosePerKg, MaxDosePerKg, Unit, Route)
- `SearchDrugCatalogQuery` : IRequest<Result<List<DrugCatalogEntryDto>>>

### Domain (Vetolib.MedicalRecords internal)

- `DrugCatalogEntry` : BaseEntity, IMultiTenant
  - ClinicId (Guid? -- null = global, non-null = clinic-specific)
  - InnName, DisplayName, Category (DrugCategory), IsActive
  - Navigation: SpeciesContraindications, Interactions, DosageGuidelines
  - Factory: `static Result<DrugCatalogEntry> Create(...)`
- `SpeciesContraindication` : value object (Species, Severity, Reason)
- `DrugInteraction` : entity (OtherDrugId, Severity, Description)
- `DosageGuideline` : value object (Species, MinDosePerKg, MaxDosePerKg, Unit, Route)

### Infrastructure

- EF Core configurations pour DrugCatalogEntry + owned types
- **Query filter special** : `WHERE ClinicId = @current OR ClinicId IS NULL`
  - ATTENTION : ceci est different du filtre standard `WHERE ClinicId = @current`
  - Ne PAS utiliser le MultiTenantDbContext standard pour ce DbSet
  - Implementer un HasQueryFilter custom sur DrugCatalogEntry dans MedicalRecordsDbContext
- Migration EF Core

### Seed Data

- Creer `DrugCatalogSeedData.cs` dans Infrastructure/
- ~200 medications : antibiotiques, NSAIDs, antiparasitaires, cardiologiques, anesthesiques
- ~50 vaccins : core + non-core pour Dog, Cat, Horse, Camel
- ~100 paires d'interactions : interactions drug-drug bien documentees
- ~30 contraindications especes : les critiques (ibuprofen/cats, permethrin/cats, xylitol/dogs, etc.)
- Dosage guidelines pour les 50 medicaments les plus prescrits, par espece
- Seed via DbInitializer avec ClinicId = null (global)
- Source : INN names (domaine public), descriptions originales

### CQRS

- `SearchDrugCatalogHandler` : recherche autocomplete par InnName/DisplayName, retourne Result<List<DrugCatalogEntryDto>>
- `GetDrugCatalogEntryByIdQuery` + handler

### Endpoints (Minimal API)

- `GET /api/medical-records/drugs?search={term}` -- autocomplete
- `GET /api/medical-records/drugs/{id}` -- detail

## MODIF_GELE

**Cette tache ne necessite PAS de modification de Shared/.** Le query filter custom est applique directement dans `MedicalRecordsDbContext.OnModelCreating()` en overridant le filtre pour le DbSet `DrugCatalogEntries`. Le MultiTenantDbContext de Shared/ n'est pas modifie.

Si cette approche ne fonctionne pas (conflit avec le filtre global de MultiTenantDbContext), creer `questions/prescriptions-catalog-multitenant-001.md` et se bloquer.

## Criteres de completion

- [ ] DrugCatalogEntry entity avec factory Result<T>
- [ ] Contracts DTOs et enums publics
- [ ] EF Core configuration avec query filter `ClinicId = @current OR ClinicId IS NULL`
- [ ] Seed data ~500 entries (200 meds + 50 vaccines + interactions + contraindications + dosages)
- [ ] SearchDrugCatalog query handler fonctionnel
- [ ] GetDrugCatalogEntryById query handler fonctionnel
- [ ] Endpoints Minimal API avec ToMinimalApiResult()
- [ ] Migration EF Core generee
- [ ] Aucune reference a un runtime d'un autre module
