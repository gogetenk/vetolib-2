using Ardalis.Result;

namespace Vetolib.AI.Contracts;

public interface ISoapNotesGenerator
{
    Task<Result<SoapNoteDto>> GenerateAsync(SoapNoteRequest request, CancellationToken ct = default);
}

public record SoapNoteRequest(
    string Species,
    string Breed,
    string PatientName,
    string Symptoms,
    string Vitals,
    string Diagnosis,
    string TreatmentPlan,
    List<string> Prescriptions);

public record SoapNoteDto(
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string Summary,
    DateTime GeneratedAt);
