# openspec/medical-records/spec.md — Module MedicalRecords

## Assemblies

```
Vetolib.MedicalRecords.Contracts/   ← public : PatientDto, OwnerDto, MedicalRecordDto, PrescriptionDto
Vetolib.MedicalRecords/             ← internal : Patient, Owner, MedicalRecord entities, handlers, DbContext
```

## Responsabilités

Dossiers médicaux des animaux : gestion des patients (animaux), propriétaires, examens, ordonnances.

---

## Entités Domain (internal)

### Owner (propriétaire)

```csharp
internal class Owner : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Phone { get; private set; }
    public string? Email { get; private set; }
    public string? EmiratesId { get; private set; }  // ID card UAE

    public static Result<Owner> Create(Guid clinicId, string firstName, string lastName, string phone) { ... }
}
```

### Patient (animal)

```csharp
internal class Patient : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string Species { get; private set; }   // Dog, Cat, Rabbit, Bird, Reptile, Rodent, Equine, Other
    public string? Breed { get; private set; }
    public string? Gender { get; private set; }   // Male, Female, Unknown
    public DateTime? DateOfBirth { get; private set; }
    public bool IsNeutered { get; private set; }
    public string? MicrochipNumber { get; private set; }   // UAE Dubai Municipality
    public bool IsDeleted { get; private set; }            // Soft delete uniquement

    public static Result<Patient> Create(...) { ... }
    public Result Update(...) { ... }
    public Result SoftDelete() { ... }
}
```

### MedicalRecord (compte-rendu d'examen)

```csharp
internal class MedicalRecord : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid AppointmentId { get; private set; }  // lié à l'appointment
    public Guid VetId { get; private set; }
    public DateTime ExaminedAt { get; private set; }
    public string ChiefComplaint { get; private set; }      // motif de consultation
    public string? ClinicalFindings { get; private set; }   // observations cliniques
    public string? Diagnosis { get; private set; }
    public string? TreatmentPlan { get; private set; }
    public decimal? Weight { get; private set; }            // kg
    public decimal? Temperature { get; private set; }       // °C
    // Les MedicalRecords ne sont jamais supprimés — pas de SoftDelete ici

    public static Result<MedicalRecord> Create(...) { ... }
    public Result AddPrescription(Prescription prescription) { ... }
}
```

### Prescription

```csharp
internal class Prescription : BaseEntity
{
    public Guid MedicalRecordId { get; private set; }
    public string VetLicenseNumber { get; private set; }  // Obligatoire légalement UAE
    public DateTime PrescribedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }       // max 6 mois
    public IReadOnlyList<PrescriptionItem> Items { get; private set; }

    public static Result<Prescription> Create(string vetLicenseNumber, List<PrescriptionItem> items) { ... }
}

internal record PrescriptionItem(
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? Instructions);
```

---

## Règles métier

- Un `MedicalRecord` est lié à un `Patient` (animal), pas au propriétaire
- Seul un `Vet` peut créer ou modifier un `MedicalRecord` ou une `Prescription`
- `Receptionist` et `Assistant` : lecture seule sur les dossiers médicaux
- **Les `MedicalRecord` ne sont jamais supprimés** (données médicales légales — pas de delete, pas de SoftDelete)
- Les `Patient` utilisent le soft delete (`IsDeleted`) — jamais de hard delete
- Une ordonnance **doit** inclure le numéro de licence vétérinaire (`VetLicenseNumber`) — obligation légale UAE
- Ordonnance valable au maximum 6 mois
- Un `Patient` peut avoir plusieurs propriétaires (divorce, copropriété) — relation N-N avec `Owner`
- `MicrochipNumber` : obligatoire pour les chiens/chats enregistrés à Dubai Municipality

---

## Endpoints

```
# Owners
POST /api/owners                            → Result<OwnerDto>                    (Vet, Admin, Receptionist)
GET  /api/owners                            → Result<PagedResult<OwnerDto>>
GET  /api/owners/{id}                       → Result<OwnerDto>
PUT  /api/owners/{id}                       → Result<OwnerDto>

# Patients
POST /api/patients                          → Result<PatientDto>                  (Vet, Admin, Receptionist)
GET  /api/patients                          → Result<PagedResult<PatientDto>>     (filtre: ownerId, species)
GET  /api/patients/{id}                     → Result<PatientDto>
PUT  /api/patients/{id}                     → Result<PatientDto>
DELETE /api/patients/{id}                   → Result                              (soft delete, Admin only)

# Medical Records
POST /api/patients/{id}/records             → Result<MedicalRecordDto>            (Vet only)
GET  /api/patients/{id}/records             → Result<IReadOnlyList<MedicalRecordDto>>
GET  /api/patients/{id}/records/{rid}       → Result<MedicalRecordDto>

# Prescriptions
POST /api/records/{rid}/prescriptions       → Result<PrescriptionDto>             (Vet only)
GET  /api/records/{rid}/prescriptions       → Result<IReadOnlyList<PrescriptionDto>>
```

---

## DTOs (Contracts, public)

```csharp
public record PatientDto(
    Guid Id, string Name, string Species, string? Breed,
    string? Gender, DateTime? DateOfBirth, bool IsNeutered,
    string? MicrochipNumber, Guid OwnerId, string OwnerName);

public record OwnerDto(
    Guid Id, string FirstName, string LastName, string Phone,
    string? Email, string? EmiratesId);

public record MedicalRecordDto(
    Guid Id, Guid PatientId, Guid AppointmentId, Guid VetId, string VetName,
    DateTime ExaminedAt, string ChiefComplaint, string? Diagnosis,
    string? TreatmentPlan, decimal? Weight, decimal? Temperature);

public record PrescriptionDto(
    Guid Id, string VetLicenseNumber, DateTime PrescribedAt,
    DateTime ExpiresAt, IReadOnlyList<PrescriptionItemDto> Items);

public record PrescriptionItemDto(
    string MedicationName, string Dosage, string Frequency,
    int DurationDays, string? Instructions);
```

---

## Domain Events (Contracts, public)

```csharp
// Optionnel pour le MVP — utile si Billing veut lier prescription à une facture
public record PrescriptionCreatedEvent(Guid PrescriptionId, Guid ClinicId) : INotification;
```

---

## Gherkins liés

`features/billing-and-records.feature` (section Medical Records)

---

## Dépendances

- `Vetolib.Shared.Kernel` + `Vetolib.Shared.Infrastructure`
- `Vetolib.Auth.Contracts` : `UserRole` (pour les checks d'autorisation)
- `Vetolib.Agenda.Contracts` : `AppointmentDto` (pour lier un record à un appointment)
