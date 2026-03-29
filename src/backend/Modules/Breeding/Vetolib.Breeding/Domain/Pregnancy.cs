using Ardalis.Result;
using Vetolib.Breeding.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Domain;

internal class Pregnancy : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? FatherPatientId { get; private set; }
    public DateOnly MatingDate { get; private set; }
    public MatingMethod MatingMethod { get; private set; }
    public DateOnly ExpectedDueDate { get; private set; }
    public DateOnly? ActualDeliveryDate { get; private set; }
    public PregnancyOutcome? Outcome { get; private set; }
    public int? OffspringCount { get; private set; }
    public PregnancyStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PregnancyCheck> _scheduledChecks = [];
    public IReadOnlyList<PregnancyCheck> ScheduledChecks => _scheduledChecks.AsReadOnly();

    private Pregnancy() { } // EF Core constructor

    public static Result<Pregnancy> Create(
        Guid clinicId,
        Guid patientId,
        Guid? fatherPatientId,
        DateOnly matingDate,
        MatingMethod matingMethod,
        string species,
        string? notes)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));
        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (errors.Count > 0)
            return Result<Pregnancy>.Invalid(errors);

        var gestationDays = GestationPeriods.GetDays(species);
        var expectedDueDate = matingDate.AddDays(gestationDays);

        return Result<Pregnancy>.Success(new Pregnancy
        {
            ClinicId = clinicId,
            PatientId = patientId,
            FatherPatientId = fatherPatientId,
            MatingDate = matingDate,
            MatingMethod = matingMethod,
            ExpectedDueDate = expectedDueDate,
            Status = PregnancyStatus.Active,
            Notes = notes
        });
    }

    public Result RecordDelivery(DateOnly deliveryDate, PregnancyOutcome outcome, int offspringCount, string? notes)
    {
        if (Status != PregnancyStatus.Active)
            return Result.Error("Cannot record delivery for a pregnancy that is not active");

        if (offspringCount < 0)
            return Result.Invalid(new ValidationError(nameof(offspringCount), "Offspring count cannot be negative"));

        ActualDeliveryDate = deliveryDate;
        Outcome = outcome;
        OffspringCount = offspringCount;
        Status = PregnancyStatus.Completed;

        if (notes is not null)
            Notes = notes;

        return Result.Success();
    }

    public Result RecordLoss(DateOnly lossDate, PregnancyOutcome outcome, string? notes)
    {
        if (Status != PregnancyStatus.Active)
            return Result.Error("Cannot record loss for a pregnancy that is not active");

        ActualDeliveryDate = lossDate;
        Outcome = outcome;
        OffspringCount = 0;
        Status = PregnancyStatus.Lost;

        if (notes is not null)
            Notes = notes;

        return Result.Success();
    }

    public Result<PregnancyCheck> ScheduleCheck(DateOnly scheduledDate, PregnancyCheckType checkType, string? note)
    {
        if (Status != PregnancyStatus.Active)
            return Result<PregnancyCheck>.Error("Cannot schedule checks for a pregnancy that is not active");

        var checkResult = PregnancyCheck.Create(Id, scheduledDate, checkType, note);
        if (!checkResult.IsSuccess)
            return checkResult;

        _scheduledChecks.Add(checkResult.Value);
        return checkResult;
    }

    public PregnancyDto ToDto() => new(
        Id,
        PatientId,
        FatherPatientId,
        MatingDate,
        MatingMethod,
        ExpectedDueDate,
        ActualDeliveryDate,
        Outcome,
        OffspringCount,
        Status,
        Notes,
        _scheduledChecks.Select(c => c.ToDto()).ToList(),
        CreatedAt,
        UpdatedAt);
}
