using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record PrescriptionOverriddenEvent(
    Guid PrescriptionId,
    Guid PatientId,
    Guid ClinicId,
    Guid VetId,
    string VetLicenseNumber,
    string OverrideJustification,
    InteractionSeverity OverrideSeverity,
    Guid? DrugCatalogEntryId,
    DateTime Timestamp) : INotification;
