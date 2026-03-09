using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private RefreshToken() { } // EF Core constructor

    public static Result<RefreshToken> Create(Guid userId, string token, int expirationDays = 7)
    {
        if (userId == Guid.Empty)
            return Result<RefreshToken>.Invalid(new ValidationError(nameof(userId), "UserId is required"));

        if (string.IsNullOrWhiteSpace(token))
            return Result<RefreshToken>.Invalid(new ValidationError(nameof(token), "Token is required"));

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false
        };

        return Result<RefreshToken>.Success(refreshToken);
    }

    public Result Revoke()
    {
        if (IsRevoked)
            return Result.Error("Token is already revoked.");

        IsRevoked = true;
        return Result.Success();
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }
}
