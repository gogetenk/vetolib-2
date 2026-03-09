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
