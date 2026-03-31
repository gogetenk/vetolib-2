using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Auth.Application.Commands.GetOrCreateReferralCode;
using Xunit;

namespace Vetolib.Tests.Unit.Auth;

public class GetOrCreateReferralCodeValidatorTests
{
    private readonly GetOrCreateReferralCodeValidator _validator = new();

    [Fact]
    public void Validate_WithValidUserId_Passes()
    {
        var command = new GetOrCreateReferralCodeCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_Fails()
    {
        var command = new GetOrCreateReferralCodeCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
