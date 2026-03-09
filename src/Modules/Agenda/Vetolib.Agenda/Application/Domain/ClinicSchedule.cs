namespace Vetolib.Agenda.Application.Domain;

internal record ClinicSchedule(TimeOnly OpeningTime, TimeOnly ClosingTime)
{
    public static ClinicSchedule Default => new(new TimeOnly(9, 0), new TimeOnly(18, 0));

    public bool IsWithinBusinessHours(TimeOnly startTime, TimeOnly endTime)
    {
        return startTime >= OpeningTime && endTime <= ClosingTime;
    }

    public string FormatHours() => $"{OpeningTime:HH'h'mm} - {ClosingTime:HH'h'mm}";
}
