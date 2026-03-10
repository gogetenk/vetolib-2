using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class OwnerPortalToken : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsentAcceptedAt { get; private set; }
    public string? ConsentVersion { get; private set; }

    private OwnerPortalToken() { } // EF Core

    public static Result<OwnerPortalToken> Create(
        Guid clinicId,
        Guid ownerId,
        string token,
        DateTime expiresAt)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (ownerId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerId), "OwnerId is required"));

        if (string.IsNullOrWhiteSpace(token))
            errors.Add(new ValidationError(nameof(token), "Token is required"));

        if (expiresAt <= DateTime.UtcNow)
            errors.Add(new ValidationError(nameof(expiresAt), "ExpiresAt must be in the future"));

        if (errors.Count > 0)
            return Result<OwnerPortalToken>.Invalid(errors);

        return Result<OwnerPortalToken>.Success(new OwnerPortalToken
        {
            ClinicId = clinicId,
            OwnerId = ownerId,
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    public bool IsValid() => ExpiresAt > DateTime.UtcNow;

    public Result RecordConsent(string consentVersion)
    {
        if (string.IsNullOrWhiteSpace(consentVersion))
            return Result.Error("INVALID_CONSENT_VERSION:Consent version is required");

        ConsentAcceptedAt = DateTime.UtcNow;
        ConsentVersion = consentVersion;
        return Result.Success();
    }

    public OwnerPortalTokenDto ToDto() => new(
        Id,
        ClinicId,
        OwnerId,
        Token,
        ExpiresAt,
        ConsentAcceptedAt,
        ConsentVersion,
        CreatedAt);
}
