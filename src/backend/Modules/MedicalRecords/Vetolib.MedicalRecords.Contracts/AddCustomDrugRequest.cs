namespace Vetolib.MedicalRecords.Contracts;

public record AddCustomDrugRequest(
    string InnName,
    string DisplayName,
    DrugCategory Category);
