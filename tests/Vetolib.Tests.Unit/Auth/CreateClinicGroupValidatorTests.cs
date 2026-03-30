using FluentAssertions;
using Vetolib.Auth.Application.Commands.CreateClinicGroup;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class CreateClinicGroupValidatorTests
{
    private readonly CreateClinicGroupValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var cmd = new CreateClinicGroupCommand("Desert Paws Group", Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var cmd = new CreateClinicGroupCommand("", Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Group name is required");
    }

    [Fact]
    public void Name_exceeding_256_chars_fails()
    {
        var cmd = new CreateClinicGroupCommand(new string('A', 257), Guid.NewGuid());
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Group name must not exceed 256 characters");
    }

    [Fact]
    public void Empty_owner_user_id_fails()
    {
        var cmd = new CreateClinicGroupCommand("Desert Paws Group", Guid.Empty);
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Owner user ID is required");
    }
}
