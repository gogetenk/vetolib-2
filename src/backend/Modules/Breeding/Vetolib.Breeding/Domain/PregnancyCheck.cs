using Ardalis.Result;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Domain;

internal class PregnancyCheck : BaseEntity
{
    public Guid PregnancyId { get; private set; }
    public DateOnly ScheduledDate { get; private set; }
    public PregnancyCheckType CheckType { get; private set; }
    public string? Note { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Result { get; private set; }

    private PregnancyCheck() { } // EF Core constructor

    public static Result<PregnancyCheck> Create(
        Guid pregnancyId,
        DateOnly scheduledDate,
        PregnancyCheckType checkType,
        string? note)
    {
        var errors = new List<ValidationError>();

        if (pregnancyId == Guid.Empty)
            errors.Add(new ValidationError(nameof(pregnancyId), "PregnancyId is required"));

        if (errors.Count > 0)
            return Result<PregnancyCheck>.Invalid(errors);

        return Result<PregnancyCheck>.Success(new PregnancyCheck
        {
            PregnancyId = pregnancyId,
            ScheduledDate = scheduledDate,
            CheckType = checkType,
            Note = note
        });
    }

    public Ardalis.Result.Result Complete(string? checkResult)
    {
        if (CompletedAt.HasValue)
            return Ardalis.Result.Result.Error("Check is already completed");

        CompletedAt = DateTime.UtcNow;
        Result = checkResult;
        return Ardalis.Result.Result.Success();
    }

    public PregnancyCheckDto ToDto() => new(
        Id,
        PregnancyId,
        ScheduledDate,
        CheckType,
        Note,
        CompletedAt,
        Result);
}
