using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// A unique referral code owned by a user (owner) for sharing with friends.
/// Global entity — not multi-tenant (referrals work across clinics).
/// </summary>
internal class ReferralCode : BaseEntity
{
    public Guid OwnerUserId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public int UsageCount { get; private set; }

    private ReferralCode() { } // EF Core constructor

    public static Result<ReferralCode> Create(Guid ownerUserId, string code)
    {
        var errors = new List<ValidationError>();

        if (ownerUserId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerUserId), "OwnerUserId is required"));

        if (string.IsNullOrWhiteSpace(code))
            errors.Add(new ValidationError(nameof(code), "Code is required"));

        if (!string.IsNullOrWhiteSpace(code) && code.Length > 20)
            errors.Add(new ValidationError(nameof(code), "Code must be 20 characters or less"));

        if (errors.Count > 0)
            return Result<ReferralCode>.Invalid(errors);

        return Result<ReferralCode>.Success(new ReferralCode
        {
            OwnerUserId = ownerUserId,
            Code = code.ToUpperInvariant(),
            UsageCount = 0
        });
    }

    public Result IncrementUsage()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No 0/O/1/I confusion
        var random = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
        var code = new char[8];
        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[random[i] % chars.Length];
        }
        return new string(code);
    }
}
