using Ardalis.Result;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// Global (non-tenant-scoped) entity representing a pet owner's portal account.
/// One OwnerAccount can be linked to Owner records across multiple clinics.
/// </summary>
internal class OwnerAccount : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }

    private OwnerAccount() { } // EF Core constructor

    public static Result<OwnerAccount> Create(string email, string phone, string fullName, string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError(nameof(email), "Email is required"));

        if (string.IsNullOrWhiteSpace(phone))
            errors.Add(new ValidationError(nameof(phone), "Phone is required"));

        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add(new ValidationError(nameof(fullName), "Full name is required"));

        var passwordErrors = ValidatePassword(password);
        errors.AddRange(passwordErrors);

        if (errors.Count > 0)
            return Result<OwnerAccount>.Invalid(errors);

        var account = new OwnerAccount
        {
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            FullName = fullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsVerified = false
        };

        return Result<OwnerAccount>.Success(account);
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public Result MarkVerified()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public OwnerAccountDto ToDto()
    {
        return new OwnerAccountDto(Id, Email, Phone, FullName, IsVerified, CreatedAt);
    }

    private static List<ValidationError> ValidatePassword(string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            errors.Add(new ValidationError("Password", "Password must be at least 8 characters"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsUpper))
            errors.Add(new ValidationError("Password", "Password must contain at least one uppercase letter"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsLower))
            errors.Add(new ValidationError("Password", "Password must contain at least one lowercase letter"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsDigit))
            errors.Add(new ValidationError("Password", "Password must contain at least one digit"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(c => !char.IsLetterOrDigit(c)))
            errors.Add(new ValidationError("Password", "Password must contain at least one special character"));

        return errors;
    }
}
