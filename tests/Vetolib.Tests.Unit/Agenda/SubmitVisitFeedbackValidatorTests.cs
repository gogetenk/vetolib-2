using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class SubmitVisitFeedbackValidatorTests
{
    private readonly SubmitVisitFeedbackValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldNotHaveErrors()
    {
        var cmd = new SubmitVisitFeedbackCommand(Guid.NewGuid(), 5, "Great!", true);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAppointmentIdEmpty_ShouldHaveError()
    {
        var cmd = new SubmitVisitFeedbackCommand(Guid.Empty, 5, null, false);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void Validate_WhenRatingOutOfRange_ShouldHaveError(int rating)
    {
        var cmd = new SubmitVisitFeedbackCommand(Guid.NewGuid(), rating, null, false);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_WhenCommentExceeds1000Chars_ShouldHaveError()
    {
        var cmd = new SubmitVisitFeedbackCommand(Guid.NewGuid(), 4, new string('a', 1001), false);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Validate_WhenCommentNull_ShouldNotHaveError()
    {
        var cmd = new SubmitVisitFeedbackCommand(Guid.NewGuid(), 4, null, false);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}
