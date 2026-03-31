using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class UserReferralTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    [Fact]
    public void SetReferrer_WithValidReferrer_Succeeds()
    {
        var user = User.Create(ValidClinicId, "new@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var referrerId = Guid.NewGuid();

        var result = user.SetReferrer(referrerId);

        result.IsSuccess.Should().BeTrue();
        user.ReferredByUserId.Should().Be(referrerId);
    }

    [Fact]
    public void SetReferrer_WithEmptyGuid_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "new@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        var result = user.SetReferrer(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_REFERRER"));
    }

    [Fact]
    public void SetReferrer_SelfReferral_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "new@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        var result = user.SetReferrer(user.Id);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SELF_REFERRAL"));
    }

    [Fact]
    public void SetReferrer_AlreadyReferred_ReturnsError()
    {
        var user = User.Create(ValidClinicId, "new@clinic.ae", "Admin1234!", UserRole.Admin).Value;
        var referrer1 = Guid.NewGuid();
        var referrer2 = Guid.NewGuid();

        user.SetReferrer(referrer1);
        var result = user.SetReferrer(referrer2);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ALREADY_REFERRED"));
    }

    [Fact]
    public void NewUser_HasNoReferrer()
    {
        var user = User.Create(ValidClinicId, "new@clinic.ae", "Admin1234!", UserRole.Admin).Value;

        user.ReferredByUserId.Should().BeNull();
    }
}
