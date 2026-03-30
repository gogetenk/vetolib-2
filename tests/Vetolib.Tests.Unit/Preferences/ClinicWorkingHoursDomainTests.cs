using FluentAssertions;
using Vetolib.Preferences.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Preferences;

public class ClinicWorkingHoursDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidOpenDay_ReturnsSuccess()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Value.IsOpen.Should().BeTrue();
        result.Value.OpenTime.Should().Be(new TimeOnly(8, 0));
        result.Value.CloseTime.Should().Be(new TimeOnly(18, 0));
        result.Value.BreakStartTime.Should().BeNull();
        result.Value.BreakEndTime.Should().BeNull();
    }

    [Fact]
    public void Create_ClosedDay_ReturnsSuccessWithDefaultTimes()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Saturday, false,
            default, default, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsOpen.Should().BeFalse();
        result.Value.OpenTime.Should().Be(default(TimeOnly));
        result.Value.CloseTime.Should().Be(default(TimeOnly));
    }

    [Fact]
    public void Create_WithSplitShift_ReturnsSuccess()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Wednesday, true,
            new TimeOnly(8, 0), new TimeOnly(20, 0),
            new TimeOnly(12, 0), new TimeOnly(16, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.BreakStartTime.Should().Be(new TimeOnly(12, 0));
        result.Value.BreakEndTime.Should().Be(new TimeOnly(16, 0));
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            Guid.Empty, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_CloseTimeBeforeOpenTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(18, 0), new TimeOnly(8, 0), null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "closeTime");
    }

    [Fact]
    public void Create_CloseTimeEqualsOpenTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(8, 0), null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "closeTime");
    }

    [Fact]
    public void Create_OnlyBreakStartProvided_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(12, 0), null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakStartTime");
    }

    [Fact]
    public void Create_OnlyBreakEndProvided_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            null, new TimeOnly(16, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakStartTime");
    }

    [Fact]
    public void Create_BreakStartBeforeOpenTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(7, 0), new TimeOnly(12, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakStartTime");
    }

    [Fact]
    public void Create_BreakEndAfterCloseTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(12, 0), new TimeOnly(19, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakEndTime");
    }

    [Fact]
    public void Create_BreakEndBeforeBreakStart_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(14, 0), new TimeOnly(12, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakEndTime");
    }

    [Fact]
    public void Create_BreakStartEqualsOpenTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(8, 0), new TimeOnly(12, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakStartTime");
    }

    [Fact]
    public void Create_BreakEndEqualsCloseTime_ReturnsInvalid()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(12, 0), new TimeOnly(18, 0));

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "breakEndTime");
    }

    [Fact]
    public void Update_WithValidData_ReturnsSuccess()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null).Value;

        var result = entry.Update(true,
            new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);

        result.IsSuccess.Should().BeTrue();
        entry.OpenTime.Should().Be(new TimeOnly(9, 0));
        entry.CloseTime.Should().Be(new TimeOnly(17, 0));
    }

    [Fact]
    public void Update_ToClosedDay_ReturnsSuccess()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null).Value;

        var result = entry.Update(false, default, default, null, null);

        result.IsSuccess.Should().BeTrue();
        entry.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Update_WithInvalidTimes_ReturnsInvalid()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null).Value;

        var result = entry.Update(true,
            new TimeOnly(18, 0), new TimeOnly(8, 0), null, null);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "closeTime");
    }

    [Fact]
    public void Update_AddSplitShift_ReturnsSuccess()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(20, 0), null, null).Value;

        var result = entry.Update(true,
            new TimeOnly(8, 0), new TimeOnly(20, 0),
            new TimeOnly(12, 0), new TimeOnly(16, 0));

        result.IsSuccess.Should().BeTrue();
        entry.BreakStartTime.Should().Be(new TimeOnly(12, 0));
        entry.BreakEndTime.Should().Be(new TimeOnly(16, 0));
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null).Value;
        var before = entry.UpdatedAt;

        entry.Update(true, new TimeOnly(9, 0), new TimeOnly(17, 0), null, null);

        entry.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void ToDto_ReturnsCorrectValues()
    {
        var entry = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Friday, true,
            new TimeOnly(8, 0), new TimeOnly(12, 0), null, null).Value;

        var dto = entry.ToDto();

        dto.DayOfWeek.Should().Be(DayOfWeek.Friday);
        dto.IsOpen.Should().BeTrue();
        dto.OpenTime.Should().Be(new TimeOnly(8, 0));
        dto.CloseTime.Should().Be(new TimeOnly(12, 0));
        dto.BreakStartTime.Should().BeNull();
        dto.BreakEndTime.Should().BeNull();
    }

    [Fact]
    public void CreateUaeDefaults_Returns7Days()
    {
        var defaults = ClinicWorkingHours.CreateUaeDefaults(ValidClinicId);

        defaults.Should().HaveCount(7);
        defaults.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
    }

    [Fact]
    public void CreateUaeDefaults_SunThuAreOpen8to18()
    {
        var defaults = ClinicWorkingHours.CreateUaeDefaults(ValidClinicId);
        var workDays = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,
            DayOfWeek.Wednesday, DayOfWeek.Thursday };

        foreach (var day in workDays)
        {
            var entry = defaults.First(r => r.Value.DayOfWeek == day).Value;
            entry.IsOpen.Should().BeTrue();
            entry.OpenTime.Should().Be(new TimeOnly(8, 0));
            entry.CloseTime.Should().Be(new TimeOnly(18, 0));
        }
    }

    [Fact]
    public void CreateUaeDefaults_FridayIsOpen8to12()
    {
        var defaults = ClinicWorkingHours.CreateUaeDefaults(ValidClinicId);
        var friday = defaults.First(r => r.Value.DayOfWeek == DayOfWeek.Friday).Value;

        friday.IsOpen.Should().BeTrue();
        friday.OpenTime.Should().Be(new TimeOnly(8, 0));
        friday.CloseTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public void CreateUaeDefaults_SaturdayIsClosed()
    {
        var defaults = ClinicWorkingHours.CreateUaeDefaults(ValidClinicId);
        var saturday = defaults.First(r => r.Value.DayOfWeek == DayOfWeek.Saturday).Value;

        saturday.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Create_ClosedDayIgnoresBreakTimes()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Saturday, false,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            new TimeOnly(12, 0), new TimeOnly(14, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.BreakStartTime.Should().BeNull();
        result.Value.BreakEndTime.Should().BeNull();
        result.Value.OpenTime.Should().Be(default(TimeOnly));
    }

    [Fact]
    public void Create_ClosedDayWithInvalidTimes_StillSucceeds()
    {
        // When closed, time validation is skipped
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Saturday, false,
            new TimeOnly(18, 0), new TimeOnly(8, 0), null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Create_SetsClinicId()
    {
        var result = ClinicWorkingHours.Create(
            ValidClinicId, DayOfWeek.Monday, true,
            new TimeOnly(8, 0), new TimeOnly(18, 0), null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ValidClinicId);
    }
}
