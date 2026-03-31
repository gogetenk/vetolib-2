using System.Security.Cryptography;
using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class SharedRecordLink : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid OwnerAccountId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public int AccessCount { get; private set; }

    private SharedRecordLink() { } // EF Core constructor

    public static Result<SharedRecordLink> Create(Guid clinicId, Guid patientId, Guid ownerAccountId)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "PatientId is required"));

        if (ownerAccountId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerAccountId), "OwnerAccountId is required"));

        if (errors.Count > 0)
            return Result<SharedRecordLink>.Invalid(errors);

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var link = new SharedRecordLink
        {
            ClinicId = clinicId,
            PatientId = patientId,
            OwnerAccountId = ownerAccountId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(72),
            AccessCount = 0
        };

        return Result<SharedRecordLink>.Success(link);
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked() => RevokedAt is not null;

    public bool IsActive() => !IsExpired() && !IsRevoked();

    public Result Revoke()
    {
        if (RevokedAt is not null)
            return Result.Error("Link has already been revoked");

        RevokedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result IncrementAccessCount()
    {
        if (!IsActive())
            return Result.Error("Link is no longer active");

        AccessCount++;
        return Result.Success();
    }
}
