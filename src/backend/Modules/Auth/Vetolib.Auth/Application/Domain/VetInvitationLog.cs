using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// Logs a pet owner's invitation to their vet to join Vetara.
/// Not tenant-scoped — this is a public portal feature.
/// Used for analytics (conversion rate tracking).
/// </summary>
internal class VetInvitationLog : BaseEntity
{
    public string VetEmail { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public string PetName { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public DateTime SentAt { get; private set; }

    /// <summary>
    /// Populated later if the vet actually registers a clinic.
    /// </summary>
    public Guid? ClinicId { get; private set; }

    private VetInvitationLog() { } // EF Core constructor

    public static Result<VetInvitationLog> Create(string vetEmail, string ownerName, string petName, string? message)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(vetEmail))
            errors.Add(new ValidationError(nameof(vetEmail), "Vet email is required"));

        if (string.IsNullOrWhiteSpace(ownerName))
            errors.Add(new ValidationError(nameof(ownerName), "Owner name is required"));

        if (string.IsNullOrWhiteSpace(petName))
            errors.Add(new ValidationError(nameof(petName), "Pet name is required"));

        if (errors.Count > 0)
            return Result<VetInvitationLog>.Invalid(errors);

        return Result<VetInvitationLog>.Success(new VetInvitationLog
        {
            VetEmail = vetEmail.Trim().ToLowerInvariant(),
            OwnerName = ownerName.Trim(),
            PetName = petName.Trim(),
            Message = message?.Trim(),
            SentAt = DateTime.UtcNow
        });
    }
}
