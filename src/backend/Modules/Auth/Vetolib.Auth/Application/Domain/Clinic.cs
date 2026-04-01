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
    public string? City { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Slug { get; private set; }
    public Contracts.SubscriptionPlan SubscriptionPlan { get; private set; }
    public DateTime TrialEndsAt { get; private set; }
    public Guid? KeycloakOrganizationId { get; private set; }

    private readonly List<string> _supportedSpecies = [];
    public IReadOnlyList<string> SupportedSpecies => _supportedSpecies.AsReadOnly();

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

        var trimmed = name.Trim();
        var clinic = new Clinic
        {
            Name = trimmed,
            Slug = GenerateSlug(trimmed),
            SubscriptionPlan = Contracts.SubscriptionPlan.Pro,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };

        return Result<Clinic>.Success(clinic);
    }

    public void SetKeycloakOrganizationId(Guid keycloakOrganizationId)
    {
        KeycloakOrganizationId = keycloakOrganizationId;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result UpdateDirectory(string? city, string? logoUrl, IEnumerable<string>? supportedSpecies)
    {
        City = city?.Trim();
        LogoUrl = logoUrl?.Trim();

        if (supportedSpecies is not null)
        {
            _supportedSpecies.Clear();
            _supportedSpecies.AddRange(supportedSpecies.Select(s => s.Trim()));
        }

        return Result.Success();
    }

    internal static string GenerateSlug(string name)
    {
        return name.Trim()
            .ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("--", "-");
    }
}
