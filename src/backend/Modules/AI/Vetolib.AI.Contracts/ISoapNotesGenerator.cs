using Ardalis.Result;

namespace Vetolib.AI.Contracts;

public interface ISoapNotesGenerator
{
    Task<Result<SoapNoteDto>> GenerateAsync(SoapNoteRequest request, CancellationToken ct = default);
}

/// <summary>
/// Supported languages for SOAP note generation.
/// </summary>
public enum SoapLanguage
{
    /// <summary>English (clinical, default).</summary>
    En,
    /// <summary>Arabic (owner-facing).</summary>
    Ar,
    /// <summary>Both English and Arabic.</summary>
    Both
}

public record SoapNoteRequest(
    string Species,
    string Breed,
    string PatientName,
    string Symptoms,
    string Vitals,
    string Diagnosis,
    string TreatmentPlan,
    List<string> Prescriptions,
    SoapLanguage Language = SoapLanguage.En);

public record SoapNoteDto(
    string Subjective,
    string Objective,
    string Assessment,
    string Plan,
    string Summary,
    DateTime GeneratedAt,
    string? SubjectiveAr = null,
    string? ObjectiveAr = null,
    string? AssessmentAr = null,
    string? PlanAr = null,
    string? SummaryAr = null);
