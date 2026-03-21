# todo-back-patients-001.md — Backend : Patients standalone

**Module** : MedicalRecords (extension)
**Dépendances** : done-back-medical-001
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `multitenant-efcore`, `aspnet-minimal-api`, `veterinary-domain`
**Gherkins** : `features/patients/patients.feature`

---

## Périmètre exact

Actuellement un patient naît implicitement lors d'un RDV. Il faut un CRUD standalone.

Endpoints :

- `GET /api/patients` — liste paginée (filtre : nom, espèce)
- `GET /api/patients/{id}` — détail + derniers RDVs + derniers dossiers médicaux
- `POST /api/patients` — créer un patient
- `PATCH /api/patients/{id}` — modifier les infos

Pas de suppression physique — un patient avec des dossiers médicaux ne peut pas être supprimé.

## Règles métier

- RECEPTIONIST et ASSISTANT : accès lecture seule (`GET` uniquement)
- VET et ADMIN : lecture + écriture
- Espèces valides UAE : Dog, Cat, Bird, Rabbit, Horse, Exotic, Camel
- Date de naissance obligatoire (calcul de l'âge en années/mois)
- Un patient appartient à un propriétaire (owner) identifié par nom + téléphone
- Isolation clinique : un patient appartient à une clinique (IMultiTenant)

## Contracts à ajouter dans `Vetolib.MedicalRecords.Contracts/`

```csharp
public record PatientDto(Guid Id, string Name, Species Species, string Breed,
    DateOnly BirthDate, string OwnerName, string OwnerPhone, Guid ClinicId);
public record CreatePatientRequest(string Name, Species Species, string Breed,
    DateOnly BirthDate, string OwnerName, string OwnerPhone);
public record PatientDetailDto(PatientDto Patient,
    IReadOnlyList<AppointmentSummaryDto> RecentAppointments,
    IReadOnlyList<MedicalRecordSummaryDto> RecentRecords);
```

## Critère de complétion

```
□ Bindings Reqnroll ROUGES avant implémentation
□ CRUD patient fonctionnel
□ RECEPTIONIST/ASSISTANT → GET only, POST/PATCH → 403
□ Espèce Camel disponible (marché UAE)
□ Patient detail inclut les derniers RDVs et dossiers
□ Tests unitaires dans Vetolib.MedicalRecords.Tests.Unit/
□ dotnet build → 0 erreur
□ Renommer en done-back-patients-001.md
```
