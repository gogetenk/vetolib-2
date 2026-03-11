using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CancelBookingAppointment;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CancelBookingAppointmentHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private readonly AgendaDbContext _context;
    private readonly CancelBookingAppointmentHandler _handler;

    public CancelBookingAppointmentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new CancelBookingAppointmentHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private Appointment CreateOwnerAppointment(
        DateOnly? date = null,
        TimeOnly? startTime = null,
        Guid? ownerId = null,
        AppointmentStatus? initialStatus = null)
    {
        // Create appointment scheduled far in the future (> 24h)
        var appointmentDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var start = startTime ?? new TimeOnly(10, 0);

        var result = Appointment.Create(
            ClinicId,
            VetId,
            "Dr. Khalid Al-Mansouri",
            AnimalId,
            "Luna",
            "Ahmed Al-Rashidi",
            appointmentDate,
            start,
            30,
            "Annual checkup",
            BookingSource.OwnerPortal,
            ownerId ?? OwnerId);

        result.IsSuccess.Should().BeTrue();
        var appointment = result.Value;

        _context.Appointments.Add(appointment);
        _context.SaveChanges();
        return appointment;
    }

    [Fact]
    public async Task Handle_WhenOwnerCancelsValidAppointment_ReturnsSuccess()
    {
        var appointment = CreateOwnerAppointment();
        var cmd = new CancelBookingAppointmentCommand(appointment.Id, OwnerId, ClinicId, "Changed plans");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var updated = await _context.Appointments.FindAsync(appointment.Id);
        updated!.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = new CancelBookingAppointmentCommand(Guid.NewGuid(), OwnerId, ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenOwnerIdDoesNotMatch_ReturnsForbidden()
    {
        var appointment = CreateOwnerAppointment(ownerId: OwnerId);
        var differentOwnerId = Guid.NewGuid();
        var cmd = new CancelBookingAppointmentCommand(appointment.Id, differentOwnerId, ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenAppointmentWithinCancellationWindow_ReturnsError()
    {
        // Appointment is less than 24h away (12h from now)
        var nearFutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(12));
        var appointment = CreateOwnerAppointment(date: nearFutureDate, startTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(12)));
        var cmd = new CancelBookingAppointmentCommand(appointment.Id, OwnerId, ClinicId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("CANCEL_WINDOW_EXPIRED"));
    }
}
