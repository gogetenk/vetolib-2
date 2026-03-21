using Ardalis.Result;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

/// <summary>
/// Template-based SOAP notes generator for development and testing.
/// Used when no LLM API key is configured.
/// Generates structured SOAP notes from the input data without AI inference.
/// </summary>
internal class TemplateSoapNotesGenerator : ISoapNotesGenerator
{
    public Task<Result<SoapNoteDto>> GenerateAsync(
        SoapNoteRequest request,
        CancellationToken ct = default)
    {
        var prescriptionText = request.Prescriptions is { Count: > 0 }
            ? string.Join("; ", request.Prescriptions)
            : "No medications prescribed at this time.";

        var subjective = $"Owner presents {request.PatientName} ({request.Species}, {request.Breed}) " +
                         $"with the following chief complaint: {request.Symptoms}.";

        var objective = !string.IsNullOrWhiteSpace(request.Vitals)
            ? $"Physical examination findings: {request.Vitals}."
            : "Physical examination findings: Within normal limits. No abnormalities detected on initial assessment.";

        var assessment = !string.IsNullOrWhiteSpace(request.Diagnosis)
            ? $"Clinical assessment: {request.Diagnosis}."
            : $"Clinical assessment: Further diagnostics recommended based on presenting symptoms ({request.Symptoms}).";

        var plan = $"Treatment plan: {(!string.IsNullOrWhiteSpace(request.TreatmentPlan) ? request.TreatmentPlan : "To be determined pending assessment.")}. " +
                   $"Medications: {prescriptionText}. " +
                   "Follow-up recommended as clinically indicated.";

        var summary = $"SOAP note for {request.PatientName} ({request.Species}/{request.Breed}) " +
                      $"presenting with {request.Symptoms}.";

        var dto = new SoapNoteDto(
            Subjective: subjective,
            Objective: objective,
            Assessment: assessment,
            Plan: plan,
            Summary: summary,
            GeneratedAt: DateTime.UtcNow);

        return Task.FromResult(Result<SoapNoteDto>.Success(dto));
    }
}
