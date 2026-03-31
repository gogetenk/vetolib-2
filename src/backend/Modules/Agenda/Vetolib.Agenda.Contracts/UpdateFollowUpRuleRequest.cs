namespace Vetolib.Agenda.Contracts;

public record UpdateFollowUpRuleRequest(
    string ConsultationType,
    int FollowUpDays,
    string FollowUpReason);
