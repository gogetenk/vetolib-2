using Ardalis.Result;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Domain;

internal record RecurrenceRule
{
    public RecurrenceFrequency Frequency { get; }
    public int Count { get; }

    private const int MaxCount = 52;
    private const int MinCount = 2;

    private RecurrenceRule(RecurrenceFrequency frequency, int count)
    {
        Frequency = frequency;
        Count = count;
    }

    public static Result<RecurrenceRule> Create(RecurrenceFrequency frequency, int count)
    {
        var errors = new List<ValidationError>();

        if (count < MinCount)
            errors.Add(new ValidationError(nameof(count), $"Count must be at least {MinCount}"));

        if (count > MaxCount)
            errors.Add(new ValidationError(nameof(count), $"Count must not exceed {MaxCount}"));

        if (!Enum.IsDefined(frequency))
            errors.Add(new ValidationError(nameof(frequency), $"Invalid frequency: {frequency}"));

        if (errors.Count > 0)
            return Result<RecurrenceRule>.Invalid(errors);

        return Result<RecurrenceRule>.Success(new RecurrenceRule(frequency, count));
    }

    public List<DateOnly> GenerateDates(DateOnly startDate)
    {
        var dates = new List<DateOnly>(Count);

        for (int i = 0; i < Count; i++)
        {
            var date = Frequency switch
            {
                RecurrenceFrequency.Daily => startDate.AddDays(i),
                RecurrenceFrequency.Weekly => startDate.AddDays(i * 7),
                RecurrenceFrequency.Biweekly => startDate.AddDays(i * 14),
                RecurrenceFrequency.Monthly => startDate.AddMonths(i),
                _ => startDate.AddDays(i * 7) // fallback to weekly
            };

            dates.Add(date);
        }

        return dates;
    }
}
