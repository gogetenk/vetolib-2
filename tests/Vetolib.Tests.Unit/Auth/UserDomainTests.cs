using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class UserDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    // ─── Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("admin@clinic.ae");
        result.Value.Role.Should().Be(UserRole.Admin);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyEmail_ReturnsInvalid()
    {
        var result = User.Create(ValidClinicId, "", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_VetWithoutLicense_ReturnsInvalid()
    {
        var result = User.Create(ValidClinicId, "vet@clinic.ae", "Vet1234!", UserRole.Vet, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "vetLicenseNumber");
    }

    [Fact]
    public void Create_VetWithLicense_ReturnsSuccess()
    {
        var result = User.Create(ValidClinicId, "vet@clinic.ae", "Vet1234!", UserRole.Vet, "VET-LIC-001");

        result.IsSuccess.Should().BeTrue();
        result.Value.VetLicenseNumber.Should().Be("VET-LIC-001");
    }

    // ─── Invite ──────────────────────────────────────────────────

    [Fact]
    public void Invite_WithValidData_ReturnsSuccess()
    {
        var result = User.Invite(ValidClinicId, "invited@clinic.ae", "Dr. Omar Khalil", "TempPass1", UserRole.Vet);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("invited@clinic.ae");
        result.Value.FullName.Should().Be("Dr. Omar Khalil");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Invite_WithEmptyFullName_ReturnsInvalid()
    {
        var result = User.Invite(ValidClinicId, "invited@clinic.ae", "", "TempPass1", UserRole.Vet);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "fullName");
    }

    [Fact]
    public void Invite_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = User.Invite(Guid.Empty, "invited@clinic.ae", "Dr. Test", "TempPass1", UserRole.Vet);

        result.IsSuccess.Should().BeFalse();
    }

    // ─── ChangeRole ──────────────────────────────────────────────

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        var user = User.Create(ValidClinicId, "recep@clinic.ae", "Recep1234!", UserRole.Receptionist).Value;

        var result = user.ChangeRole(UserRole.Assistant);

        result.IsSuccess.Should().BeTrue();
        user.Role.Should().Be(UserRole.Assistant);
    }

    // ─── Deactivate ──────────────────────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = User.Create(ValidClinicId, "vet@clinic.ae", "Vet1234!", UserRole.Vet, "VET-LIC-001").Value;

        var result = user.Deactivate();

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    // ─── IsActive default ────────────────────────────────────────

    [Fact]
    public void Create_NewUser_IsActiveByDefault()
    {
        var userResult = User.Create(ValidClinicId, "new@clinic.ae", "New1234!", UserRole.Receptionist);

        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.IsActive.Should().BeTrue();
    }

    // ─── MustChangePassword ────────────────────────────────────────

    [Fact]
    public void Create_NewUser_MustChangePasswordIsFalse()
    {
        var result = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value.MustChangePassword.Should().BeFalse();
    }

    [Fact]
    public void Invite_NewUser_MustChangePasswordIsTrue()
    {
        var result = User.Invite(ValidClinicId, "invited@clinic.ae", "Dr. Omar", "TempPass1", UserRole.Vet);

        result.IsSuccess.Should().BeTrue();
        result.Value.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public void ChangePassword_ClearsMustChangePasswordFlag()
    {
        var user = User.Invite(ValidClinicId, "invited@clinic.ae", "Dr. Omar", "TempPass1", UserRole.Vet).Value;

        user.MustChangePassword.Should().BeTrue();

        var changeResult = user.ChangePassword("TempPass1", "NewPass1234!");

        changeResult.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeFalse();
    }

    // ─── EmailVerification ────────────────────────────────────────

    [Fact]
    public void Create_NewUser_EmailVerifiedIsFalse()
    {
        var result = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_NewUser_HasVerificationToken()
    {
        var result = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmailVerificationToken.Should().NotBeNullOrEmpty();
        result.Value.EmailVerificationExpiry.Should().NotBeNull();
        result.Value.EmailVerificationExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void VerifyEmail_WithValidToken_SetsEmailVerified()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var token = user.EmailVerificationToken!;

        var result = user.VerifyEmail(token);

        result.IsSuccess.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationExpiry.Should().BeNull();
    }

    [Fact]
    public void VerifyEmail_WithInvalidToken_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        var result = user.VerifyEmail("wrong-token");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TOKEN"));
    }

    [Fact]
    public void VerifyEmail_AlreadyVerified_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var token = user.EmailVerificationToken!;
        user.VerifyEmail(token);

        var result = user.VerifyEmail(token);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EMAIL_ALREADY_VERIFIED"));
    }

    [Fact]
    public void RegenerateVerificationToken_WhenNotVerified_GeneratesNewToken()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var originalToken = user.EmailVerificationToken;

        var result = user.RegenerateVerificationToken();

        result.IsSuccess.Should().BeTrue();
        user.EmailVerificationToken.Should().NotBe(originalToken);
        user.EmailVerificationExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void RegenerateVerificationToken_WhenAlreadyVerified_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        user.VerifyEmail(user.EmailVerificationToken!);

        var result = user.RegenerateVerificationToken();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("EMAIL_ALREADY_VERIFIED"));
    }

    [Fact]
    public void ToDto_IncludesEmailVerified()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var dto = user.ToDto();
        dto.EmailVerified.Should().BeFalse();

        user.VerifyEmail(user.EmailVerificationToken!);
        var dto2 = user.ToDto();
        dto2.EmailVerified.Should().BeTrue();
    }

    // ─── AuthProvider ─────────────────────────────────────────────

    [Fact]
    public void Create_LegacyUser_HasAuthProviderLegacy()
    {
        var result = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuthProvider.Should().Be(AuthProvider.Legacy);
    }

    [Fact]
    public void CreateKeycloakUser_WithValidData_ReturnsSuccess()
    {
        var keycloakId = Guid.NewGuid();
        var result = User.CreateKeycloakUser(ValidClinicId, "kc@clinic.ae", UserRole.Admin, keycloakId);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuthProvider.Should().Be(AuthProvider.Keycloak);
        result.Value.PasswordHash.Should().BeNull();
        result.Value.KeycloakUserId.Should().Be(keycloakId);
        result.Value.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void CreateKeycloakUser_WithEmptyKeycloakId_ReturnsInvalid()
    {
        var result = User.CreateKeycloakUser(ValidClinicId, "kc@clinic.ae", UserRole.Admin, Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "keycloakUserId");
    }

    [Fact]
    public void CreateKeycloakUser_VetWithoutLicense_ReturnsInvalid()
    {
        var result = User.CreateKeycloakUser(ValidClinicId, "vet@clinic.ae", UserRole.Vet, Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "vetLicenseNumber");
    }

    [Fact]
    public void VerifyPassword_LegacyUser_ReturnsTrue()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        var result = user.VerifyPassword("Admin1234!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_LegacyUser_WrongPassword_ReturnsFalse()
    {
        var user = User.Create(ValidClinicId, "admin@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        var result = user.VerifyPassword("WrongPass1!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_KeycloakUser_ReturnsError()
    {
        var user = User.CreateKeycloakUser(ValidClinicId, "kc@clinic.ae", UserRole.Admin, Guid.NewGuid()).Value;

        var result = user.VerifyPassword("AnyPass1!");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("KEYCLOAK_AUTH_REQUIRED"));
    }

    [Fact]
    public void ChangePassword_KeycloakUser_ReturnsError()
    {
        var user = User.CreateKeycloakUser(ValidClinicId, "kc@clinic.ae", UserRole.Admin, Guid.NewGuid()).Value;

        var result = user.ChangePassword("Old1234!", "New1234!");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("KEYCLOAK_AUTH_REQUIRED"));
    }

    // ─── ToListItemDto ───────────────────────────────────────────

    [Fact]
    public void ToListItemDto_ReturnsCorrectData()
    {
        var user = User.Invite(ValidClinicId, "dr@clinic.ae", "Dr. Fatima Al-Rashidi", "Pass1", UserRole.Vet).Value;

        var dto = user.ToListItemDto();

        dto.Email.Should().Be("dr@clinic.ae");
        dto.FullName.Should().Be("Dr. Fatima Al-Rashidi");
        dto.Role.Should().Be(UserRole.Vet);
        dto.IsActive.Should().BeTrue();
    }
}
