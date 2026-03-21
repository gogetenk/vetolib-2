using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class WhatsAppBusinessAccount : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string WabaId { get; private set; } = string.Empty;
    public string PhoneNumberId { get; private set; } = string.Empty;

    /// <summary>
    /// Encrypted access token. Never stored in plain text.
    /// </summary>
    public string EncryptedAccessToken { get; private set; } = string.Empty;

    private WhatsAppBusinessAccount() { } // EF Core

    public static Result<WhatsAppBusinessAccount> Create(
        Guid clinicId,
        string wabaId,
        string phoneNumberId,
        string encryptedAccessToken)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(wabaId))
            errors.Add(new ValidationError(nameof(wabaId), "WABA ID is required"));

        if (string.IsNullOrWhiteSpace(phoneNumberId))
            errors.Add(new ValidationError(nameof(phoneNumberId), "Phone Number ID is required"));

        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            errors.Add(new ValidationError(nameof(encryptedAccessToken), "Access Token is required"));

        if (errors.Count > 0)
            return Result<WhatsAppBusinessAccount>.Invalid(errors);

        return Result<WhatsAppBusinessAccount>.Success(new WhatsAppBusinessAccount
        {
            ClinicId = clinicId,
            WabaId = wabaId,
            PhoneNumberId = phoneNumberId,
            EncryptedAccessToken = encryptedAccessToken
        });
    }

    public Result Update(string wabaId, string phoneNumberId, string encryptedAccessToken)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(wabaId))
            errors.Add(new ValidationError(nameof(wabaId), "WABA ID is required"));

        if (string.IsNullOrWhiteSpace(phoneNumberId))
            errors.Add(new ValidationError(nameof(phoneNumberId), "Phone Number ID is required"));

        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            errors.Add(new ValidationError(nameof(encryptedAccessToken), "Access Token is required"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        WabaId = wabaId;
        PhoneNumberId = phoneNumberId;
        EncryptedAccessToken = encryptedAccessToken;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Contracts.WhatsAppConfigDto ToDto() => new(
        Id,
        WabaId,
        PhoneNumberId,
        HasAccessToken: !string.IsNullOrEmpty(EncryptedAccessToken),
        CreatedAt,
        UpdatedAt);
}
