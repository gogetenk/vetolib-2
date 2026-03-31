using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class StaffScheduleDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly MorningStart = new(8, 0);
    private static readonly TimeOnly MorningEnd = new(14, 0);

    [Fact]
    public void Create_WhenValid_ReturnsSuccess()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.UserId.Should().Be(UserId);
        result.Value.UserName.Should().Be("Dr. Fatima Al-Zahra");
        result.Value.Date.Should().Be(FutureDate);
        result.Value.StartTime.Should().Be(MorningStart);
        result.Value.EndTime.Should().Be(MorningEnd);
        result.Value.ShiftType.Should().Be(ShiftType.Morning);
        result.Value.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenIsAvailableFalse_SetsTimeOff()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning, isAvailable: false);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Create_WhenEmptyClinicId_ReturnsInvalid()
    {
        var result = StaffSchedule.Create(
            Guid.Empty, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WhenEmptyUserId_ReturnsInvalid()
    {
        var result = StaffSchedule.Create(
            ClinicId, Guid.Empty, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "userId");
    }

    [Fact]
    public void Create_WhenEmptyUserName_ReturnsInvalid()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "userName");
    }

    [Fact]
    public void Create_WhenEndTimeBeforeStartTime_ReturnsInvalid()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            new TimeOnly(14, 0), new TimeOnly(8, 0), ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "endTime");
    }

    [Fact]
    public void Create_WhenEndTimeEqualsStartTime_ReturnsInvalid()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningStart, ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "endTime");
    }

    [Fact]
    public void Create_WhenMultipleErrors_ReturnsAllValidationErrors()
    {
        var result = StaffSchedule.Create(
            Guid.Empty, Guid.Empty, "", FutureDate,
            new TimeOnly(14, 0), new TimeOnly(8, 0), ShiftType.Morning);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCountGreaterOrEqualTo(4);
    }

    [Fact]
    public void ToDto_MapsAllProperties()
    {
        var result = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            MorningStart, MorningEnd, ShiftType.Morning);

        var dto = result.Value.ToDto();

        dto.Id.Should().Be(result.Value.Id);
        dto.ClinicId.Should().Be(ClinicId);
        dto.UserId.Should().Be(UserId);
        dto.UserName.Should().Be("Dr. Fatima Al-Zahra");
        dto.Date.Should().Be(FutureDate);
        dto.StartTime.Should().Be(MorningStart);
        dto.EndTime.Should().Be(MorningEnd);
        dto.ShiftType.Should().Be(ShiftType.Morning);
        dto.IsAvailable.Should().BeTrue();
    }
}
