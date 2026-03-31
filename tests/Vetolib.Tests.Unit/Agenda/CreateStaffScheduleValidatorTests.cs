using FluentAssertions;
using FluentValidation.TestHelper;
using Vetolib.Agenda.Application.Commands.CreateStaffSchedule;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateStaffScheduleValidatorTests
{
    private readonly CreateStaffScheduleValidator _validator = new();

    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private CreateStaffScheduleCommand BuildCommand(
        Guid? clinicId = null,
        Guid? userId = null,
        string? userName = null) =>
        new(
            ClinicId: clinicId ?? ClinicId,
            UserId: userId ?? UserId,
            UserName: userName ?? "Dr. Fatima Al-Zahra",
            Date: FutureDate,
            StartTime: new TimeOnly(8, 0),
            EndTime: new TimeOnly(14, 0),
            ShiftType: ShiftType.Morning,
            IsAvailable: true);

    [Fact]
    public void Validate_WhenValid_HasNoErrors()
    {
        var result = _validator.TestValidate(BuildCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenEmptyClinicId_HasError()
    {
        var result = _validator.TestValidate(BuildCommand(clinicId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.ClinicId);
    }

    [Fact]
    public void Validate_WhenEmptyUserId_HasError()
    {
        var result = _validator.TestValidate(BuildCommand(userId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenEmptyUserName_HasError()
    {
        var result = _validator.TestValidate(BuildCommand(userName: ""));
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }
}
