using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class OnboardingState : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool WelcomeBannerDismissed { get; private set; }
    public bool ChecklistDismissed { get; private set; }
    public List<string> CompletedSteps { get; private set; } = [];
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private OnboardingState() { } // EF Core constructor

    public static Result<OnboardingState> Create(Guid userId, string role, Guid clinicId)
    {
        var errors = new List<ValidationError>();

        if (userId == Guid.Empty)
            errors.Add(new ValidationError(nameof(userId), "UserId is required"));

        if (string.IsNullOrWhiteSpace(role))
            errors.Add(new ValidationError(nameof(role), "Role is required"));

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (errors.Count > 0)
            return Result<OnboardingState>.Invalid(errors);

        return Result<OnboardingState>.Success(new OnboardingState
        {
            UserId = userId,
            Role = role,
            ClinicId = clinicId,
            WelcomeBannerDismissed = false,
            ChecklistDismissed = false,
            CompletedSteps = [],
            StartedAt = DateTimeOffset.UtcNow
        });
    }

    public Result DismissBanner()
    {
        WelcomeBannerDismissed = true;
        return Result.Success();
    }

    public Result DismissChecklist()
    {
        ChecklistDismissed = true;
        CompletedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result CompleteStep(string stepId)
    {
        if (string.IsNullOrWhiteSpace(stepId))
            return Result.Invalid(new ValidationError(nameof(stepId), "StepId is required"));

        if (!CompletedSteps.Contains(stepId))
        {
            CompletedSteps.Add(stepId);
        }

        return Result.Success();
    }

    public bool IsFullyCompleted(IReadOnlyList<string> roleSteps)
    {
        return roleSteps.All(s => CompletedSteps.Contains(s));
    }

    public Result MarkCompleted()
    {
        if (!CompletedAt.HasValue)
            CompletedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
