using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Auth.Application.Commands.InviteVet;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class InviteVetValidatorTests
{
    private readonly InviteVetValidator _validator = new();

    [Fact]
    public void Valid_Command_Passes()
    {
        var command = new InviteVetCommand("vet@clinic.ae", "Fatima", "Luna", "Please join!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    public void Invalid_Email_Fails(string? email)
    {
        var command = new InviteVetCommand(email!, "Owner", "Pet", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.VetEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("A")] // too short — min 2
    public void Invalid_OwnerName_Fails(string? ownerName)
    {
        var command = new InviteVetCommand("vet@clinic.ae", ownerName!, "Pet", null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OwnerName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Invalid_PetName_Fails(string? petName)
    {
        var command = new InviteVetCommand("vet@clinic.ae", "Owner", petName!, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PetName);
    }

    [Fact]
    public void Null_Message_Passes()
    {
        var command = new InviteVetCommand("vet@clinic.ae", "Owner", "Pet", null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Message_TooLong_Fails()
    {
        var command = new InviteVetCommand("vet@clinic.ae", "Owner", "Pet", new string('a', 1001));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }
}
