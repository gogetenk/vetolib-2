using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class WhatsAppPhoneMapping : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }

    /// <summary>
    /// Phone number in E.164 format (e.g., +971501234567).
    /// </summary>
    public string Phone { get; private set; } = string.Empty;

    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Date when the pet owner opted in to WhatsApp notifications.
    /// </summary>
    public DateTime OptInDate { get; private set; }

    private WhatsAppPhoneMapping() { } // EF Core

    public static Result<WhatsAppPhoneMapping> Create(
        Guid clinicId,
        Guid ownerId,
        string phone)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (ownerId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerId), "OwnerId is required"));

        if (string.IsNullOrWhiteSpace(phone))
            errors.Add(new ValidationError(nameof(phone), "Phone is required"));
        else if (!phone.StartsWith('+') || phone.Length < 8)
            errors.Add(new ValidationError(nameof(phone), "Phone must be in E.164 format"));

        if (errors.Count > 0)
            return Result<WhatsAppPhoneMapping>.Invalid(errors);

        return Result<WhatsAppPhoneMapping>.Success(new WhatsAppPhoneMapping
        {
            ClinicId = clinicId,
            OwnerId = ownerId,
            Phone = phone,
            OptInDate = DateTime.UtcNow
        });
    }
}
