using System.Text.Json;
using Ardalis.Result;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

/// <summary>
/// Generates veterinary SOAP notes using an LLM via <see cref="IChatClient"/>.
/// The underlying model can be Claude, GPT, Ollama, or any IChatClient-compatible provider.
/// Supports English, Arabic, and bilingual output.
/// </summary>
internal class ClaudeSoapNotesGenerator : ISoapNotesGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPromptEn = """
        You are an expert veterinary medical records assistant specializing in SOAP note generation.
        Your role is to produce structured, professional, and clinically accurate SOAP notes
        for veterinary consultations.

        SOAP FORMAT:
        - Subjective (S): Owner-reported symptoms, history, chief complaint, duration, onset.
          Use professional veterinary language. Include the patient name and species context.
        - Objective (O): Physical examination findings, vital signs, diagnostic results.
          Present data in a structured, measurable format.
        - Assessment (A): Clinical assessment, differential diagnoses ranked by likelihood,
          confirmed diagnosis if applicable. Use proper veterinary medical terminology.
        - Plan (P): Treatment plan including medications with dosages, follow-up schedule,
          client education, monitoring instructions, and prognosis.

        RULES:
        - Use professional veterinary medical terminology throughout.
        - Be concise but thorough — each section should be 2-5 sentences.
        - Include species-specific considerations (e.g., drug safety for cats vs dogs).
        - List prescriptions with standard veterinary notation (drug, dose, route, frequency, duration).
        - Generate a brief 1-2 sentence summary suitable for medical record overview.
        - All content must be in English.

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "subjective": "<string>",
          "objective": "<string>",
          "assessment": "<string>",
          "plan": "<string>",
          "summary": "<string, 1-2 sentence overview>"
        }

        Do not include any text outside the JSON object.
        """;

    private const string SystemPromptAr = """
        أنت مساعد خبير في السجلات الطبية البيطرية متخصص في إنشاء ملاحظات SOAP.
        دورك هو إنتاج ملاحظات SOAP منظمة ومهنية ودقيقة سريرياً للاستشارات البيطرية.

        تنسيق SOAP:
        - الشكوى الذاتية (S): الأعراض التي أبلغ عنها المالك، التاريخ المرضي، الشكوى الرئيسية، المدة، البداية.
          استخدم لغة بيطرية مهنية. أدرج اسم المريض وسياق النوع.
        - الفحص الموضوعي (O): نتائج الفحص السريري، العلامات الحيوية، نتائج التشخيص.
          قدّم البيانات بتنسيق منظم وقابل للقياس.
        - التقييم (A): التقييم السريري، التشخيصات التفاضلية مرتبة حسب الاحتمالية،
          التشخيص المؤكد إن وُجد. استخدم المصطلحات الطبية البيطرية الصحيحة.
        - الخطة (P): خطة العلاج بما في ذلك الأدوية بالجرعات، جدول المتابعة،
          تثقيف العميل، تعليمات المراقبة، والتوقعات.

        القواعد:
        - استخدم المصطلحات الطبية البيطرية المهنية في جميع الأقسام.
        - كن موجزاً لكن شاملاً — يجب أن يتكون كل قسم من 2-5 جمل.
        - أدرج الاعتبارات الخاصة بالنوع (مثل سلامة الأدوية للقطط مقابل الكلاب).
        - اكتب الوصفات بالتدوين البيطري القياسي (الدواء، الجرعة، طريقة الإعطاء، التكرار، المدة).
        - أنشئ ملخصاً موجزاً من جملة أو جملتين مناسباً لنظرة عامة على السجل الطبي.
        - يجب أن يكون كل المحتوى باللغة العربية.

        أجب فقط بكائن JSON صالح يطابق هذا المخطط بالضبط:
        {
          "subjective": "<string>",
          "objective": "<string>",
          "assessment": "<string>",
          "plan": "<string>",
          "summary": "<string, ملخص من جملة أو جملتين>"
        }

        لا تُدرج أي نص خارج كائن JSON.
        """;

    private const string SystemPromptBoth = """
        You are an expert veterinary medical records assistant specializing in SOAP note generation.
        Your role is to produce structured, professional, and clinically accurate SOAP notes
        for veterinary consultations in BOTH English and Arabic.

        SOAP FORMAT:
        - Subjective (S): Owner-reported symptoms, history, chief complaint, duration, onset.
        - Objective (O): Physical examination findings, vital signs, diagnostic results.
        - Assessment (A): Clinical assessment, differential diagnoses, confirmed diagnosis if applicable.
        - Plan (P): Treatment plan including medications with dosages, follow-up schedule, prognosis.

        RULES:
        - Use professional veterinary medical terminology throughout.
        - Be concise but thorough — each section should be 2-5 sentences.
        - Include species-specific considerations.
        - List prescriptions with standard veterinary notation.
        - Generate a brief 1-2 sentence summary suitable for medical record overview.
        - English fields: clinical English.
        - Arabic fields: professional Arabic veterinary terminology.

        Respond ONLY with a valid JSON object matching this exact schema:
        {
          "subjective": "<string in English>",
          "objective": "<string in English>",
          "assessment": "<string in English>",
          "plan": "<string in English>",
          "summary": "<string in English>",
          "subjective_ar": "<string in Arabic>",
          "objective_ar": "<string in Arabic>",
          "assessment_ar": "<string in Arabic>",
          "plan_ar": "<string in Arabic>",
          "summary_ar": "<string in Arabic>"
        }

        Do not include any text outside the JSON object.
        """;

    private readonly IChatClient _chatClient;
    private readonly ILogger<ClaudeSoapNotesGenerator> _logger;

    public ClaudeSoapNotesGenerator(
        IChatClient chatClient,
        ILogger<ClaudeSoapNotesGenerator> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<Result<SoapNoteDto>> GenerateAsync(
        SoapNoteRequest request,
        CancellationToken ct = default)
    {
        var systemPrompt = request.Language switch
        {
            SoapLanguage.Ar => SystemPromptAr,
            SoapLanguage.Both => SystemPromptBoth,
            _ => SystemPromptEn
        };

        var userMessage = BuildUserMessage(request);

        ChatResponse completion;
        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, userMessage)
            };

            var options = new ChatOptions
            {
                Temperature = 0.2f,
                ResponseFormat = ChatResponseFormat.Json
            };

            completion = await _chatClient.GetResponseAsync(messages, options, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "AI service call failed for SOAP note generation.");
            return Result<SoapNoteDto>.Unavailable("AI_SERVICE_UNAVAILABLE");
        }

        var rawResponse = completion.Text ?? string.Empty;

        if (request.Language == SoapLanguage.Both)
        {
            return ParseBilingualResponse(rawResponse);
        }

        return ParseSingleLanguageResponse(rawResponse, request.Language);
    }

    private Result<SoapNoteDto> ParseSingleLanguageResponse(string rawResponse, SoapLanguage language)
    {
        SoapAiResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<SoapAiResponse>(rawResponse, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI SOAP response: {Response}", rawResponse);
            return Result<SoapNoteDto>.Error("AI_RESPONSE_INVALID");
        }

        if (parsed is null ||
            string.IsNullOrWhiteSpace(parsed.Subjective) ||
            string.IsNullOrWhiteSpace(parsed.Objective) ||
            string.IsNullOrWhiteSpace(parsed.Assessment) ||
            string.IsNullOrWhiteSpace(parsed.Plan))
        {
            _logger.LogError("AI SOAP response missing required sections: {Response}", rawResponse);
            return Result<SoapNoteDto>.Error("AI_RESPONSE_INVALID");
        }

        var dto = new SoapNoteDto(
            Subjective: parsed.Subjective,
            Objective: parsed.Objective,
            Assessment: parsed.Assessment,
            Plan: parsed.Plan,
            Summary: parsed.Summary ?? string.Empty,
            GeneratedAt: DateTime.UtcNow);

        return Result<SoapNoteDto>.Success(dto);
    }

    private Result<SoapNoteDto> ParseBilingualResponse(string rawResponse)
    {
        SoapAiBilingualResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<SoapAiBilingualResponse>(rawResponse, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse bilingual AI SOAP response: {Response}", rawResponse);
            return Result<SoapNoteDto>.Error("AI_RESPONSE_INVALID");
        }

        if (parsed is null ||
            string.IsNullOrWhiteSpace(parsed.Subjective) ||
            string.IsNullOrWhiteSpace(parsed.Objective) ||
            string.IsNullOrWhiteSpace(parsed.Assessment) ||
            string.IsNullOrWhiteSpace(parsed.Plan) ||
            string.IsNullOrWhiteSpace(parsed.SubjectiveAr) ||
            string.IsNullOrWhiteSpace(parsed.ObjectiveAr) ||
            string.IsNullOrWhiteSpace(parsed.AssessmentAr) ||
            string.IsNullOrWhiteSpace(parsed.PlanAr))
        {
            _logger.LogError("Bilingual AI SOAP response missing required sections: {Response}", rawResponse);
            return Result<SoapNoteDto>.Error("AI_RESPONSE_INVALID");
        }

        var dto = new SoapNoteDto(
            Subjective: parsed.Subjective,
            Objective: parsed.Objective,
            Assessment: parsed.Assessment,
            Plan: parsed.Plan,
            Summary: parsed.Summary ?? string.Empty,
            GeneratedAt: DateTime.UtcNow,
            SubjectiveAr: parsed.SubjectiveAr,
            ObjectiveAr: parsed.ObjectiveAr,
            AssessmentAr: parsed.AssessmentAr,
            PlanAr: parsed.PlanAr,
            SummaryAr: parsed.SummaryAr);

        return Result<SoapNoteDto>.Success(dto);
    }

    private static string BuildUserMessage(SoapNoteRequest request)
    {
        var parts = new List<string>
        {
            $"Patient: {request.PatientName}",
            $"Species: {request.Species}",
            $"Breed: {request.Breed}",
            $"Chief complaint / Symptoms: {request.Symptoms}"
        };

        if (!string.IsNullOrWhiteSpace(request.Vitals))
            parts.Add($"Vital signs: {request.Vitals}");

        if (!string.IsNullOrWhiteSpace(request.Diagnosis))
            parts.Add($"Diagnosis: {request.Diagnosis}");

        if (!string.IsNullOrWhiteSpace(request.TreatmentPlan))
            parts.Add($"Treatment plan: {request.TreatmentPlan}");

        if (request.Prescriptions is { Count: > 0 })
            parts.Add($"Prescriptions: {string.Join("; ", request.Prescriptions)}");

        return string.Join("\n", parts);
    }

    private sealed record SoapAiResponse(
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? Summary);

    private sealed record SoapAiBilingualResponse(
        string Subjective,
        string Objective,
        string Assessment,
        string Plan,
        string? Summary,
        string? SubjectiveAr,
        string? ObjectiveAr,
        string? AssessmentAr,
        string? PlanAr,
        string? SummaryAr);
}
