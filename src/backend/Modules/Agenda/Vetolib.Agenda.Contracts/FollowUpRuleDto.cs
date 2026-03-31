namespace Vetolib.Agenda.Contracts;

public record FollowUpRuleDto(
    Guid Id,
    string ConsultationType,
    int FollowUpDays,
    string FollowUpReason,
    bool IsActive);
