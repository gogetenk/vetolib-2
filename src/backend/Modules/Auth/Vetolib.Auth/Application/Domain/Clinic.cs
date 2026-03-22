using Ardalis.Result;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// Represents a veterinary clinic tenant.
/// Cross-tenant entity: NOT IMultiTenant — stored without ClinicId filter.
/// </summary>
internal class Clinic : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Contracts.SubscriptionPlan SubscriptionPlan { get; private set; }
    public DateTime TrialEndsAt { get; private set; }

    private Clinic() { } // EF Core constructor

    /// <summary>
    /// Creates a new clinic with a configurable Pro trial period.
    /// </summary>
    public static Result<Clinic> Create(string name, int trialDays = 14)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "ClinicName is required"));

        if (errors.Count > 0)
            return Result<Clinic>.Invalid(errors);

        var clinic = new Clinic
        {
            Name = name.Trim(),
            SubscriptionPlan = Contracts.SubscriptionPlan.Pro,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };

        return Result<Clinic>.Success(clinic);
    }
}
