using FluentValidation.TestHelper;
using Vetolib.Messaging.Application.Commands.MarkAsSpam;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class MarkAsSpamValidatorTests
{
    private readonly MarkAsSpamValidator _validator = new();

    [Fact]
    public void Should_have_error_when_ConversationId_is_empty_guid()
    {
        var command = new MarkAsSpamCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ConversationId)
              .WithErrorMessage("ConversationId is required.");
    }

    [Fact]
    public void Should_not_have_error_when_ConversationId_is_valid_guid()
    {
        var command = new MarkAsSpamCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.ConversationId);
    }
}
