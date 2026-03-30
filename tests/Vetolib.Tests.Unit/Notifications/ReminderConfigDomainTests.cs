using FluentAssertions;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class ReminderConfigDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void CreateDefault_WithValidClinicId_ReturnsSuccess()
    {
        var result = ReminderConfig.CreateDefault(ClinicId);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.Appointment24hEnabled.Should().BeTrue();
        result.Value.VaccinationDueEnabled.Should().BeTrue();
        result.Value.FollowUpEnabled.Should().BeTrue();
        result.Value.Appointment24hLeadTimeHours.Should().Be(24);
        result.Value.VaccinationDueLeadTimeDays.Should().Be(7);
        result.Value.PreferredReminderChannel.Should().Be(ReminderChannel.Email);
    }

    [Fact]
    public void CreateDefault_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = ReminderConfig.CreateDefault(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Update_WithValidValues_ReturnsSuccess()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(
            appointment24hEnabled: false,
            vaccinationDueEnabled: true,
            followUpEnabled: false,
            appointment24hLeadTimeHours: 48,
            vaccinationDueLeadTimeDays: 14,
            preferredReminderChannel: ReminderChannel.WhatsApp);

        result.IsSuccess.Should().BeTrue();
        config.Appointment24hEnabled.Should().BeFalse();
        config.VaccinationDueEnabled.Should().BeTrue();
        config.FollowUpEnabled.Should().BeFalse();
        config.Appointment24hLeadTimeHours.Should().Be(48);
        config.VaccinationDueLeadTimeDays.Should().Be(14);
        config.PreferredReminderChannel.Should().Be(ReminderChannel.WhatsApp);
    }

    [Fact]
    public void Update_WithLeadTimeBelow1_ReturnsInvalid()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 0, 7, ReminderChannel.Email);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Update_WithLeadTimeAbove72_ReturnsInvalid()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 73, 7, ReminderChannel.Email);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithVaccinationLeadTimeBelow1_ReturnsInvalid()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 24, 0, ReminderChannel.Email);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithVaccinationLeadTimeAbove30_ReturnsInvalid()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 24, 31, ReminderChannel.Email);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_DisablesAllReminders_ReturnsSuccess()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(false, false, false, 24, 7, ReminderChannel.Email);

        result.IsSuccess.Should().BeTrue();
        config.Appointment24hEnabled.Should().BeFalse();
        config.VaccinationDueEnabled.Should().BeFalse();
        config.FollowUpEnabled.Should().BeFalse();
    }

    [Fact]
    public void Update_WithBothChannel_ReturnsSuccess()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 24, 7, ReminderChannel.Both);

        result.IsSuccess.Should().BeTrue();
        config.PreferredReminderChannel.Should().Be(ReminderChannel.Both);
    }

    [Fact]
    public void Update_WithInvalidChannel_ReturnsInvalid()
    {
        var config = ReminderConfig.CreateDefault(ClinicId).Value;

        var result = config.Update(true, true, true, 24, 7, (ReminderChannel)99);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }
}
