using FluentAssertions;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Notifications.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Notifications;

public class ReminderLogDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AppointmentId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "ahmed@example.com",
            ClinicId,
            appointmentId: AppointmentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReminderType.Should().Be(ReminderType.Appointment24h);
        result.Value.Channel.Should().Be(NotificationChannel.Email);
        result.Value.RecipientEmail.Should().Be("ahmed@example.com");
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.AppointmentId.Should().Be(AppointmentId);
        result.Value.DeliveryStatus.Should().Be(DeliveryStatus.Pending);
    }

    [Fact]
    public void Create_WithEmptyEmail_ReturnsInvalid()
    {
        var result = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "",
            ClinicId);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsSuccess()
    {
        // ClinicId can be Guid.Empty for cross-module logging (e.g., AppointmentReminderConsumer)
        var result = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "ahmed@example.com",
            Guid.Empty);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Create_WithPatientId_SetsPatientId()
    {
        var result = ReminderLog.Create(
            ReminderType.VaccinationDue,
            NotificationChannel.Email,
            "ahmed@example.com",
            ClinicId,
            patientId: PatientId);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.ReminderType.Should().Be(ReminderType.VaccinationDue);
    }

    [Fact]
    public void MarkSent_SetsDeliveryStatusToSent()
    {
        var log = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "ahmed@example.com",
            ClinicId).Value;

        log.MarkSent();

        log.DeliveryStatus.Should().Be(DeliveryStatus.Sent);
    }

    [Fact]
    public void MarkFailed_SetsDeliveryStatusToFailed()
    {
        var log = ReminderLog.Create(
            ReminderType.Appointment24h,
            NotificationChannel.Email,
            "ahmed@example.com",
            ClinicId).Value;

        log.MarkFailed();

        log.DeliveryStatus.Should().Be(DeliveryStatus.Failed);
    }
}
