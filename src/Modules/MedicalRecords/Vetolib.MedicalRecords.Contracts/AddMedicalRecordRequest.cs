namespace Vetolib.MedicalRecords.Contracts;

public record AddMedicalRecordRequest(
    string Diagnosis,
    string Treatment);
