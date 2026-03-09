namespace Vetolib.Agenda.Application.Services;

internal class SlotScoringOptions
{
    public double GapMinimizationWeight { get; set; } = 0.30;
    public double LoadBalancingWeight { get; set; } = 0.25;
    public double TypeGroupingWeight { get; set; } = 0.20;
    public double VetPreferenceWeight { get; set; } = 0.15;
    public double ClientProximityWeight { get; set; } = 0.10;
}

internal class AgendaWorkingHoursOptions
{
    public int StartHour { get; set; } = 9;
    public int EndHour { get; set; } = 18;
}

internal class AgendaOptions
{
    public SlotScoringOptions SlotScoring { get; set; } = new();
    public AgendaWorkingHoursOptions WorkingHours { get; set; } = new();
    public Dictionary<string, int> DefaultDurationByType { get; set; } = new();
}
