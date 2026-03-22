using Ardalis.Result;
using Vetolib.Auth.Application.Domain.Events;
using Vetolib.Auth.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

internal class User : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? VetLicenseNumber { get; private set; }
    public bool IsLocked { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool IsActive { get; private set; } = true;

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
            errors.Add(new ValidationError(nameof(vetLicenseNumber), "A veterinary license number is required for the Vet role"));

        if (errors.Count > 0)
            return Result<User>.Invalid(errors);

        var user = new User
        {
            ClinicId = clinicId,
            Email = email.ToLowerInvariant(),
            FullName = string.Empty,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            VetLicenseNumber = vetLicenseNumber,
            IsLocked = false,
            FailedLoginAttempts = 0,
            IsActive = true
        };

        return Result<User>.Success(user);
    }

    public static Result<User> Invite(Guid clinicId, string email, string fullName, string temporaryPassword, UserRole role, string clinicName = "Vetolib Clinic")
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError(nameof(email), "Email is required"));

        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add(new ValidationError(nameof(fullName), "Full name is required"));

        if (errors.Count > 0)
            return Result<User>.Invalid(errors);

        var user = new User
        {
            ClinicId = clinicId,
            Email = email.ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
            Role = role,
            IsLocked = false,
            FailedLoginAttempts = 0,
            IsActive = true
        };

        user.AddDomainEvent(new UserInvitedDomainEvent(user.Id, user.Email, user.FullName, temporaryPassword, clinicName));

        return Result<User>.Success(user);
    }

    public Result ChangeRole(UserRole newRole)
    {
        Role = newRole;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public Result RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
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

    public Result ChangePassword(string currentPassword, string newPassword)
    {
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, PasswordHash))
            return Result.Error("INVALID_CURRENT_PASSWORD");

        var passwordErrors = ValidatePassword(newPassword);
        if (passwordErrors.Count > 0)
            return Result.Invalid(passwordErrors);

        PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
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

    public UserListItemDto ToListItemDto()
    {
        return new UserListItemDto(Id, Email, FullName, Role, IsActive);
    }

    private static List<ValidationError> ValidatePassword(string password)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            errors.Add(new ValidationError("Password", "Password must contain at least 8 characters"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsUpper))
            errors.Add(new ValidationError("Password", "Password must contain at least one uppercase letter"));

        if (!string.IsNullOrWhiteSpace(password) && !password.Any(char.IsDigit))
            errors.Add(new ValidationError("Password", "Password must contain at least one digit"));

        return errors;
    }
}
