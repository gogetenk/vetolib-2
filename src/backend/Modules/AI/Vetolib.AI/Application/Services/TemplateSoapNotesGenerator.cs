using Ardalis.Result;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

/// <summary>
/// Template-based SOAP notes generator for development and testing.
/// Used when no LLM API key is configured.
/// Generates structured SOAP notes from the input data without AI inference.
/// Supports English, Arabic, and bilingual output.
/// </summary>
internal class TemplateSoapNotesGenerator : ISoapNotesGenerator
{
    public Task<Result<SoapNoteDto>> GenerateAsync(
        SoapNoteRequest request,
        CancellationToken ct = default)
    {
        var enDto = request.Language is SoapLanguage.En or SoapLanguage.Both
            ? GenerateEnglish(request)
            : null;

        var arDto = request.Language is SoapLanguage.Ar or SoapLanguage.Both
            ? GenerateArabic(request)
            : null;

        // When language is En: English fields populated, Arabic fields null
        // When language is Ar: English fields hold Arabic text (primary), Arabic fields null
        // When language is Both: English fields populated, Arabic fields populated
        var dto = request.Language switch
        {
            SoapLanguage.En => new SoapNoteDto(
                Subjective: enDto!.Subjective,
                Objective: enDto!.Objective,
                Assessment: enDto!.Assessment,
                Plan: enDto!.Plan,
                Summary: enDto!.Summary,
                GeneratedAt: DateTime.UtcNow),

            SoapLanguage.Ar => new SoapNoteDto(
                Subjective: arDto!.Subjective,
                Objective: arDto!.Objective,
                Assessment: arDto!.Assessment,
                Plan: arDto!.Plan,
                Summary: arDto!.Summary,
                GeneratedAt: DateTime.UtcNow),

            SoapLanguage.Both => new SoapNoteDto(
                Subjective: enDto!.Subjective,
                Objective: enDto!.Objective,
                Assessment: enDto!.Assessment,
                Plan: enDto!.Plan,
                Summary: enDto!.Summary,
                GeneratedAt: DateTime.UtcNow,
                SubjectiveAr: arDto!.Subjective,
                ObjectiveAr: arDto!.Objective,
                AssessmentAr: arDto!.Assessment,
                PlanAr: arDto!.Plan,
                SummaryAr: arDto!.Summary),

            _ => throw new ArgumentOutOfRangeException(nameof(request.Language))
        };

        return Task.FromResult(Result<SoapNoteDto>.Success(dto));
    }

    private static SoapSections GenerateEnglish(SoapNoteRequest request)
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

        return new SoapSections(subjective, objective, assessment, plan, summary);
    }

    private static SoapSections GenerateArabic(SoapNoteRequest request)
    {
        var prescriptionText = request.Prescriptions is { Count: > 0 }
            ? string.Join("؛ ", request.Prescriptions)
            : "لم يتم وصف أي أدوية في الوقت الحالي.";

        var subjective = $"يُحضر المالك {request.PatientName} ({request.Species}، {request.Breed}) " +
                         $"مع الشكوى الرئيسية التالية: {request.Symptoms}.";

        var objective = !string.IsNullOrWhiteSpace(request.Vitals)
            ? $"نتائج الفحص السريري: {request.Vitals}."
            : "نتائج الفحص السريري: ضمن الحدود الطبيعية. لم يتم اكتشاف أي تشوهات في التقييم الأولي.";

        var assessment = !string.IsNullOrWhiteSpace(request.Diagnosis)
            ? $"التقييم السريري: {request.Diagnosis}."
            : $"التقييم السريري: يُوصى بإجراء فحوصات إضافية بناءً على الأعراض المُقدَّمة ({request.Symptoms}).";

        var plan = $"خطة العلاج: {(!string.IsNullOrWhiteSpace(request.TreatmentPlan) ? request.TreatmentPlan : "سيتم تحديدها بعد التقييم.")}. " +
                   $"الأدوية: {prescriptionText}. " +
                   "يُوصى بالمتابعة حسب الحالة السريرية.";

        var summary = $"ملاحظات SOAP لـ {request.PatientName} ({request.Species}/{request.Breed}) " +
                      $"بسبب {request.Symptoms}.";

        return new SoapSections(subjective, objective, assessment, plan, summary);
    }

    private sealed record SoapSections(
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string Summary);
}
