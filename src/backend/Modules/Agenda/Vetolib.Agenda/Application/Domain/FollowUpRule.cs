using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class FollowUpRule : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string ConsultationType { get; private set; } = string.Empty;
    public int FollowUpDays { get; private set; }
    public string FollowUpReason { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private FollowUpRule() { } // EF Core constructor

    public static Result<FollowUpRule> Create(
        Guid clinicId,
        string consultationType,
        int followUpDays,
        string followUpReason)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(consultationType))
            errors.Add(new ValidationError(nameof(consultationType), "ConsultationType is required"));
        else if (consultationType.Length > 100)
            errors.Add(new ValidationError(nameof(consultationType), "ConsultationType must not exceed 100 characters"));

        if (followUpDays < 1 || followUpDays > 365)
            errors.Add(new ValidationError(nameof(followUpDays), "FollowUpDays must be between 1 and 365"));

        if (string.IsNullOrWhiteSpace(followUpReason))
            errors.Add(new ValidationError(nameof(followUpReason), "FollowUpReason is required"));
        else if (followUpReason.Length > 200)
            errors.Add(new ValidationError(nameof(followUpReason), "FollowUpReason must not exceed 200 characters"));

        if (errors.Count > 0)
            return Result<FollowUpRule>.Invalid(errors);

        return Result<FollowUpRule>.Success(new FollowUpRule
        {
            ClinicId = clinicId,
            ConsultationType = consultationType.Trim(),
            FollowUpDays = followUpDays,
            FollowUpReason = followUpReason.Trim(),
            IsActive = true
        });
    }

    public Result Update(string consultationType, int followUpDays, string followUpReason)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(consultationType))
            errors.Add(new ValidationError(nameof(consultationType), "ConsultationType is required"));
        else if (consultationType.Length > 100)
            errors.Add(new ValidationError(nameof(consultationType), "ConsultationType must not exceed 100 characters"));

        if (followUpDays < 1 || followUpDays > 365)
            errors.Add(new ValidationError(nameof(followUpDays), "FollowUpDays must be between 1 and 365"));

        if (string.IsNullOrWhiteSpace(followUpReason))
            errors.Add(new ValidationError(nameof(followUpReason), "FollowUpReason is required"));
        else if (followUpReason.Length > 200)
            errors.Add(new ValidationError(nameof(followUpReason), "FollowUpReason must not exceed 200 characters"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        ConsultationType = consultationType.Trim();
        FollowUpDays = followUpDays;
        FollowUpReason = followUpReason.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Error("FollowUpRule is already inactive.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public FollowUpRuleDto ToDto() =>
        new(Id, ConsultationType, FollowUpDays, FollowUpReason, IsActive);
}
