using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class AppointmentDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly ValidStart = new(10, 0);
    private const int ValidDuration = 30;

    private static Result<Appointment> CreateAppointment(BookingSource source = BookingSource.Staff)
        => Appointment.Create(
            ClinicId,
            VetId,
            "Dr. Khalid Al-Mansouri",
            AnimalId,
            "Luna",
            "Ahmed Al-Rashidi",
            "ahmed@email.ae",
            FutureDate,
            ValidStart,
            ValidDuration,
            "Annual checkup",
            source);

    // ── BookingSource ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WhenNoSourceProvided_DefaultsToStaff()
    {
        var result = CreateAppointment();

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(BookingSource.Staff);
    }

    [Fact]
    public void Create_WhenSourceIsOwnerPortal_SetsOwnerPortalSource()
    {
        var result = CreateAppointment(BookingSource.OwnerPortal);

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(BookingSource.OwnerPortal);
    }

    // ── IncrementReschedule ────────────────────────────────────────────────────────

    [Fact]
    public void IncrementReschedule_WhenBelowMax_IncrementsCounter()
    {
        var appointment = CreateAppointment().Value;
        var newAppointmentId = Guid.NewGuid();

        var result = appointment.IncrementReschedule(newAppointmentId, maxReschedules: 3);

        result.IsSuccess.Should().BeTrue();
        appointment.RescheduleCount.Should().Be(1);
    }

    [Fact]
    public void IncrementReschedule_WhenMaxReached_ReturnsError()
    {
        var appointment = CreateAppointment().Value;
        var newAppointmentId = Guid.NewGuid();

        // Reach the max
        appointment.IncrementReschedule(newAppointmentId, maxReschedules: 2);
        appointment.IncrementReschedule(newAppointmentId, maxReschedules: 2);

        // Attempt beyond max
        var result = appointment.IncrementReschedule(newAppointmentId, maxReschedules: 2);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("RESCHEDULE_LIMIT_REACHED"));
        appointment.RescheduleCount.Should().Be(2);
    }

    [Fact]
    public void Create_WhenCreated_RescheduleCountIsZero()
    {
        var result = CreateAppointment();

        result.IsSuccess.Should().BeTrue();
        result.Value.RescheduleCount.Should().Be(0);
    }

    [Fact]
    public void Create_WhenCreated_OriginalAppointmentIdIsNull()
    {
        var result = CreateAppointment();

        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalAppointmentId.Should().BeNull();
    }
}
