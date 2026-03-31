namespace Vetolib.Agenda.Contracts;

public record CreateFollowUpRuleRequest(
    string ConsultationType,
    int FollowUpDays,
    string FollowUpReason);
