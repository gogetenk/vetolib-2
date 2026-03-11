using FluentValidation.TestHelper;
using Vetolib.Messaging.Application.Commands.DeleteTemplate;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class DeleteTemplateValidatorTests
{
    private readonly DeleteTemplateValidator _validator = new();

    [Fact]
    public void Should_have_error_when_Id_is_empty_guid()
    {
        var command = new DeleteTemplateCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Template Id is required.");
    }

    [Fact]
    public void Should_not_have_error_when_Id_is_valid_guid()
    {
        var command = new DeleteTemplateCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
