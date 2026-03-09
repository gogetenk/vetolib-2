using Ardalis.Result;
using Vetolib.AI.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.AI.Application.Domain;

internal class TriageResult : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Symptoms { get; private set; } = string.Empty;
    public string Species { get; private set; } = string.Empty;
    public string? Breed { get; private set; }
    public int? AgeMonths { get; private set; }
    public decimal? WeightKg { get; private set; }
    public AISeverity SuggestedSeverity { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public string RecommendedSpecialty { get; private set; } = string.Empty;
    public string Reasoning { get; private set; } = string.Empty;
    public double Confidence { get; private set; }
    public bool WasAccepted { get; private set; }
    public AISeverity? OverriddenSeverity { get; private set; }
    public string ModelUsed { get; private set; } = string.Empty;
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public long LatencyMs { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // EF Core constructor
    private TriageResult() { }

    public static Result<TriageResult> Create(
        Guid clinicId,
        string symptoms,
        string species,
        string? breed,
        int? ageMonths,
        decimal? weightKg,
        AISeverity suggestedSeverity,
        int estimatedDurationMinutes,
        string recommendedSpecialty,
        string reasoning,
        double confidence,
        string modelUsed,
        int promptTokens,
        int completionTokens,
        long latencyMs,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(symptoms))
            return Result<TriageResult>.Invalid(new ValidationError(nameof(Symptoms), "Symptoms are required."));

        if (string.IsNullOrWhiteSpace(species))
            return Result<TriageResult>.Invalid(new ValidationError(nameof(Species), "Species is required."));

        var entity = new TriageResult
        {
            ClinicId = clinicId,
            Symptoms = symptoms,
            Species = species,
            Breed = breed,
            AgeMonths = ageMonths,
            WeightKg = weightKg,
            SuggestedSeverity = suggestedSeverity,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            RecommendedSpecialty = recommendedSpecialty,
            Reasoning = reasoning,
            Confidence = confidence,
            WasAccepted = false,
            OverriddenSeverity = null,
            ModelUsed = modelUsed,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            LatencyMs = latencyMs,
            CreatedBy = createdBy
        };

        return Result<TriageResult>.Success(entity);
    }

    public Result Accept()
    {
        WasAccepted = true;
        OverriddenSeverity = null;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Override(AISeverity overriddenSeverity)
    {
        WasAccepted = false;
        OverriddenSeverity = overriddenSeverity;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
