using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class WaitingRoomTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly ValidStart = new(10, 0);
    private const int ValidDuration = 30;

    private static Appointment CreateScheduledAppointment()
        => Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", "ahmed@email.ae",
            FutureDate, ValidStart, ValidDuration, "Annual checkup").Value;

    // ── MarkWaitingRoom ─────────────────────────────────────────────────────

    [Fact]
    public void MarkWaitingRoom_WhenScheduled_TransitionsToWaitingRoom()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.MarkWaitingRoom();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.WaitingRoom);
        appointment.WaitingRoomAt.Should().NotBeNull();
        appointment.WaitingRoomAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkWaitingRoom_WhenScheduled_RaisesPatientArrivedEvent()
    {
        var appointment = CreateScheduledAppointment();

        appointment.MarkWaitingRoom();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PatientArrivedEvent>()
            .Which.Should().Match<PatientArrivedEvent>(e =>
                e.AppointmentId == appointment.Id &&
                e.VeterinarianId == VetId &&
                e.PatientName == "Luna" &&
                e.OwnerName == "Ahmed Al-Rashidi");
    }

    [Fact]
    public void MarkWaitingRoom_WhenAlreadyCheckedIn_ReturnsError()
    {
        var appointment = CreateScheduledAppointment();
        appointment.CheckIn();

        var result = appointment.MarkWaitingRoom();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public void MarkWaitingRoom_WhenCompleted_ReturnsError()
    {
        var appointment = CreateScheduledAppointment();
        appointment.CheckIn();
        appointment.StartConsultation();
        appointment.Complete();

        var result = appointment.MarkWaitingRoom();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public void MarkWaitingRoom_WhenCancelled_ReturnsError()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Cancel();

        var result = appointment.MarkWaitingRoom();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    // ── CheckIn from WaitingRoom ────────────────────────────────────────────

    [Fact]
    public void CheckIn_WhenInWaitingRoom_TransitionsToCheckedIn()
    {
        var appointment = CreateScheduledAppointment();
        appointment.MarkWaitingRoom();

        var result = appointment.CheckIn();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    [Fact]
    public void CheckIn_WhenScheduled_StillWorks()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.CheckIn();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    // ── Full flow: Scheduled → WaitingRoom → CheckedIn → InProgress → Completed ─

    [Fact]
    public void FullFlow_WithWaitingRoom_CompletesSuccessfully()
    {
        var appointment = CreateScheduledAppointment();

        appointment.MarkWaitingRoom().IsSuccess.Should().BeTrue();
        appointment.CheckIn().IsSuccess.Should().BeTrue();
        appointment.StartConsultation().IsSuccess.Should().BeTrue();
        appointment.Complete().IsSuccess.Should().BeTrue();

        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    // ── NoShow from WaitingRoom ─────────────────────────────────────────────

    [Fact]
    public void MarkNoShow_WhenInWaitingRoom_Succeeds()
    {
        var appointment = CreateScheduledAppointment();
        appointment.MarkWaitingRoom();

        var result = appointment.MarkNoShow();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }
}
