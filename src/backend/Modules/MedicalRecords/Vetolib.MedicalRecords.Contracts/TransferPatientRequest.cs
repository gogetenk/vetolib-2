namespace Vetolib.MedicalRecords.Contracts;

public record TransferPatientRequest(
    Guid TargetClinicId,
    bool IncludeRecords = true,
    bool IncludeWeightHistory = true);
