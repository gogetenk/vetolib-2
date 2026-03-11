using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.RescheduleBookingAppointment;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class RescheduleBookingAppointmentHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    // New slot date in the future (no conflict)
    private static readonly DateOnly NewDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
    private static readonly TimeOnly NewStartTime = new(14, 0);
    private const int NewDurationMinutes = 30;

    private readonly AgendaDbContext _context;
    private readonly RescheduleBookingAppointmentHandler _handler;

    public RescheduleBookingAppointmentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new RescheduleBookingAppointmentHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private Appointment CreateOwnerAppointment(
        int? rescheduleCount = null,
        Guid? ownerId = null,
        DateOnly? date = null)
    {
        var appointmentDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var result = Appointment.Create(
            ClinicId,
            VetId,
            "Dr. Khalid Al-Mansouri",
            AnimalId,
            "Luna",
            "Ahmed Al-Rashidi",
            appointmentDate,
            new TimeOnly(10, 0),
            30,
            "Annual checkup",
            BookingSource.OwnerPortal,
            ownerId ?? OwnerId);

        result.IsSuccess.Should().BeTrue();
        var appointment = result.Value;

        // Pre-set reschedule count by calling IncrementReschedule
        if (rescheduleCount.HasValue)
        {
            for (int i = 0; i < rescheduleCount.Value; i++)
                appointment.IncrementReschedule(Guid.Empty, maxReschedules: 10);
        }

        _context.Appointments.Add(appointment);
        _context.SaveChanges();
        return appointment;
    }

    [Fact]
    public async Task Handle_WhenOwnerReschedulesValidAppointment_ReturnsNewAppointment()
    {
        var appointment = CreateOwnerAppointment();
        var cmd = new RescheduleBookingAppointmentCommand(
            appointment.Id, OwnerId, ClinicId, NewDate, NewStartTime, NewDurationMinutes);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.Scheduled);
        result.Value.Source.Should().Be(BookingSource.OwnerPortal);
        result.Value.OriginalAppointmentId.Should().Be(appointment.Id);

        // Original appointment should be cancelled
        var original = await _context.Appointments.FindAsync(appointment.Id);
        original!.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WhenOwnerIdDoesNotMatch_ReturnsForbidden()
    {
        var appointment = CreateOwnerAppointment(ownerId: OwnerId);
        var differentOwnerId = Guid.NewGuid();
        var cmd = new RescheduleBookingAppointmentCommand(
            appointment.Id, differentOwnerId, ClinicId, NewDate, NewStartTime, NewDurationMinutes);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenMaxReschedulesReached_ReturnsError()
    {
        // Pre-set reschedule count to 2 (the default max)
        var appointment = CreateOwnerAppointment(rescheduleCount: 2);
        var cmd = new RescheduleBookingAppointmentCommand(
            appointment.Id, OwnerId, ClinicId, NewDate, NewStartTime, NewDurationMinutes);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("RESCHEDULE_LIMIT_REACHED"));
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = new RescheduleBookingAppointmentCommand(
            Guid.NewGuid(), OwnerId, ClinicId, NewDate, NewStartTime, NewDurationMinutes);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}
