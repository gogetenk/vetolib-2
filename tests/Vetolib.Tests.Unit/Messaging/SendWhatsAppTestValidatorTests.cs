using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Messaging.Application.Commands.SendWhatsAppTest;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class SendWhatsAppTestValidatorTests
{
    private readonly SendWhatsAppTestValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var cmd = new SendWhatsAppTestCommand("+971501234567", "hello_world");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyPhone_ShouldFail()
    {
        var cmd = new SendWhatsAppTestCommand("", "hello_world");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.RecipientPhone);
    }

    [Fact]
    public void Validate_InvalidPhoneFormat_ShouldFail()
    {
        var cmd = new SendWhatsAppTestCommand("not-a-phone", "hello_world");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.RecipientPhone);
    }

    [Fact]
    public void Validate_EmptyTemplateName_ShouldFail()
    {
        var cmd = new SendWhatsAppTestCommand("+971501234567", "");
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.TemplateName);
    }
}
