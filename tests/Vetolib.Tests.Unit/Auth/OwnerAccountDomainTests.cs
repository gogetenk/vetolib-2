using Ardalis.Result;
using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class OwnerAccountDomainTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsSuccess()
    {
        var result = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima Al Rashid", "SecureP@ss1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("fatima@example.com");
        result.Value.Phone.Should().Be("+971501234567");
        result.Value.FullName.Should().Be("Fatima Al Rashid");
        result.Value.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_EmptyEmail_ReturnsInvalid()
    {
        var result = OwnerAccount.Create("", "+971501234567", "Fatima", "SecureP@ss1");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "email");
    }

    [Fact]
    public void Create_EmptyPhone_ReturnsInvalid()
    {
        var result = OwnerAccount.Create("fatima@example.com", "", "Fatima", "SecureP@ss1");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "phone");
    }

    [Fact]
    public void Create_EmptyFullName_ReturnsInvalid()
    {
        var result = OwnerAccount.Create("fatima@example.com", "+971501234567", "", "SecureP@ss1");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "fullName");
    }

    [Fact]
    public void Create_WeakPassword_ReturnsInvalid()
    {
        var result = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "short");

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "Password");
    }

    [Fact]
    public void Create_NormalizesEmail()
    {
        var result = OwnerAccount.Create("  FATIMA@Example.COM  ", "+971501234567", "Fatima", "SecureP@ss1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("fatima@example.com");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;

        account.VerifyPassword("SecureP@ss1").Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;

        account.VerifyPassword("WrongP@ss1").Should().BeFalse();
    }

    [Fact]
    public void MarkVerified_SetsIsVerifiedTrue()
    {
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima", "SecureP@ss1").Value;

        var result = account.MarkVerified();

        result.IsSuccess.Should().BeTrue();
        account.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void ToDto_ReturnsCorrectDto()
    {
        var account = OwnerAccount.Create("fatima@example.com", "+971501234567", "Fatima Al Rashid", "SecureP@ss1").Value;

        var dto = account.ToDto();

        dto.Id.Should().Be(account.Id);
        dto.Email.Should().Be("fatima@example.com");
        dto.Phone.Should().Be("+971501234567");
        dto.FullName.Should().Be("Fatima Al Rashid");
        dto.IsVerified.Should().BeFalse();
    }
}
