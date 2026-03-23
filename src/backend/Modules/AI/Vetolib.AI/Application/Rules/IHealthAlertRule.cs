using Vetolib.AI.Application.Domain;

namespace Vetolib.AI.Application.Rules;

internal interface IHealthAlertRule
{
    string RuleId { get; }
    IReadOnlyList<HealthAlert> Evaluate(PatientAlertContext patient, IReadOnlyList<HealthAlert> existingAlerts);
}
