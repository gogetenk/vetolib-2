using FluentAssertions;
using Vetolib.Auth.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class VetInvitationLogDomainTests
{
    [Fact]
    public void Create_ValidInputs_ReturnsSuccess()
    {
        var result = VetInvitationLog.Create("vet@clinic.ae", "Fatima", "Luna", "Please join!");

        result.IsSuccess.Should().BeTrue();
        result.Value.VetEmail.Should().Be("vet@clinic.ae");
        result.Value.OwnerName.Should().Be("Fatima");
        result.Value.PetName.Should().Be("Luna");
        result.Value.Message.Should().Be("Please join!");
        result.Value.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_NormalizesEmail_ToLowerCase()
    {
        var result = VetInvitationLog.Create("VET@CLINIC.AE", "Owner", "Pet", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.VetEmail.Should().Be("vet@clinic.ae");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = VetInvitationLog.Create("  vet@test.ae  ", "  Owner  ", "  Pet  ", "  Message  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.VetEmail.Should().Be("vet@test.ae");
        result.Value.OwnerName.Should().Be("Owner");
        result.Value.PetName.Should().Be("Pet");
        result.Value.Message.Should().Be("Message");
    }

    [Fact]
    public void Create_NullMessage_Allowed()
    {
        var result = VetInvitationLog.Create("vet@test.ae", "Owner", "Pet", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Owner", "Pet")]
    [InlineData("vet@test.ae", "", "Pet")]
    [InlineData("vet@test.ae", "Owner", "")]
    public void Create_MissingRequiredField_ReturnsInvalid(string email, string ownerName, string petName)
    {
        var result = VetInvitationLog.Create(email, ownerName, petName, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }
}
