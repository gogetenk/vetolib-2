using System.Text.Json;
using Ardalis.Result;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Vetolib.AI.Contracts;

namespace Vetolib.AI.Application.Services;

/// <summary>
/// Generates veterinary SOAP notes using an LLM via <see cref="IChatClient"/>.
/// The underlying model can be Claude, GPT, Ollama, or any IChatClient-compatible provider.
/// </summary>
internal class ClaudeSoapNotesGenerator : ISoapNotesGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt = """
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
        var userMessage = BuildUserMessage(request);

        ChatResponse completion;
        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt),
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
}
