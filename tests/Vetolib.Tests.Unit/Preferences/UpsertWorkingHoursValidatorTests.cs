using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Preferences.Application.Commands.UpsertWorkingHours;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class UpsertWorkingHoursValidatorTests
{
    private readonly UpsertWorkingHoursValidator _validator = new();

    [Fact]
    public void Valid_Command_Passes()
    {
        var command = new UpsertWorkingHoursCommand(
            Guid.NewGuid(),
            [new WorkingHoursItemRequest(DayOfWeek.Monday, true,
                new TimeOnly(8, 0), new TimeOnly(18, 0), null, null)]);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyClinicId_Fails()
    {
        var command = new UpsertWorkingHoursCommand(
            Guid.Empty,
            [new WorkingHoursItemRequest(DayOfWeek.Monday, true,
                new TimeOnly(8, 0), new TimeOnly(18, 0), null, null)]);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ClinicId);
    }

    [Fact]
    public void EmptyDays_Fails()
    {
        var command = new UpsertWorkingHoursCommand(
            Guid.NewGuid(),
            []);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Days);
    }

    [Fact]
    public void DuplicateDays_Fails()
    {
        var command = new UpsertWorkingHoursCommand(
            Guid.NewGuid(),
            [
                new WorkingHoursItemRequest(DayOfWeek.Monday, true,
                    new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
                new WorkingHoursItemRequest(DayOfWeek.Monday, false,
                    default, default, null, null)
            ]);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Days);
    }

    [Fact]
    public void MultipleDifferentDays_Passes()
    {
        var command = new UpsertWorkingHoursCommand(
            Guid.NewGuid(),
            [
                new WorkingHoursItemRequest(DayOfWeek.Monday, true,
                    new TimeOnly(8, 0), new TimeOnly(18, 0), null, null),
                new WorkingHoursItemRequest(DayOfWeek.Tuesday, true,
                    new TimeOnly(9, 0), new TimeOnly(17, 0), null, null)
            ]);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
