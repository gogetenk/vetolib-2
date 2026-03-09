namespace Vetolib.MedicalRecords.Application.Commands.ImportPatients;

/// <summary>
/// Represents a single row from the CSV import file.
/// Expected columns: PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone
/// </summary>
internal class CsvPatientRow
{
    public string PatientName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
}
