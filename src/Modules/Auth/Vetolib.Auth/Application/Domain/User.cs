using Ardalis.Result;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class User : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? VetLicenseNumber { get; private set; }
    public bool IsLocked { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    private User() { } // EF Core constructor

    public static Result<User> Create(Guid clinicId, string email, string password, UserRole role, string? vetLicenseNumber = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError(nameof(email), "Email is required"));

        var passwordErrors = ValidatePassword(password);
        errors.AddRange(passwordErrors);

        if (role == UserRole.Vet && string.IsNullOrWhiteSpace(vetLicenseNumber))
            errors.Add(new ValidationError(nameof(vetLicenseNumber), "Un numero de licence veterinaire est requis pour le role Vet"));

        if (errors.Count > 0)
            return Result<User>.Invalid(errors);

        var user = new User
        {
            ClinicId = clinicId,
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            VetLicenseNumber = vetLicenseNumber,
            IsLocked = false,
            FailedLoginAttempts = 0
        };

        return Result<User>.Success(user);
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
        return Result.Success();
    }

    public Result Unlock()
    {
        IsLocked = false;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        return Result.Success();
    }

    public bool IsCurrentlyLocked()
    {
        if (!IsLocked) return false;
        if (LockedUntil.HasValue && LockedUntil.Value <= DateTime.UtcNow)
        {
            // Lock has expired
            return false;
        }
        return true;
    }

    public void ResetFailedAttempts()
    {
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
    }

    public UserDto ToDto()
    {
        return new UserDto(Id, Email, Role, ClinicId, VetLicenseNumber);
    }

    private static List<ValidationError> ValidatePassword(string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            errors.Add(new ValidationError("Password", "Le mot de passe doit contenir au moins 8 caracteres"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsUpper))
            errors.Add(new ValidationError("Password", "Le mot de passe doit contenir au moins une majuscule"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsDigit))
            errors.Add(new ValidationError("Password", "Le mot de passe doit contenir au moins un chiffre"));

        return errors;
    }
}
