using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CancelAppointmentSeries;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CancelAppointmentSeriesHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid SeriesId = new("44444444-4444-4444-4444-444444444444");

    private readonly AgendaDbContext _context;
    private readonly CancelAppointmentSeriesHandler _handler;

    public CancelAppointmentSeriesHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new CancelAppointmentSeriesHandler(_context);
    }

    private Appointment CreateSeriesAppointment(DateOnly date)
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            date, new TimeOnly(10, 0), 30, "Follow-up").Value;
        appointment.AssignToSeries(SeriesId);
        return appointment;
    }

    [Fact]
    public async Task Handle_WithFutureScheduledAppointments_CancelsAll()
    {
        var futureDate1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var futureDate2 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));

        _context.Appointments.AddRange(
            CreateSeriesAppointment(futureDate1),
            CreateSeriesAppointment(futureDate2));
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new CancelAppointmentSeriesCommand(SeriesId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().AllSatisfy(a => a.Status.Should().Be(AppointmentStatus.Cancelled));
    }

    [Fact]
    public async Task Handle_WithPastAppointmentsInSeries_OnlyCancelsFuture()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var pastAppt = CreateSeriesAppointment(pastDate);
        var futureAppt = CreateSeriesAppointment(futureDate);

        _context.Appointments.AddRange(pastAppt, futureAppt);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new CancelAppointmentSeriesCommand(SeriesId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);

        var appointments = await _context.Appointments.ToListAsync();
        var past = appointments.First(a => a.Date == pastDate);
        var future = appointments.First(a => a.Date == futureDate);
        past.Status.Should().Be(AppointmentStatus.Scheduled);
        future.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WithNoMatchingSeries_ReturnsNotFound()
    {
        var unknownSeriesId = Guid.NewGuid();

        var result = await _handler.Handle(new CancelAppointmentSeriesCommand(unknownSeriesId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAllAlreadyCancelled_ReturnsNotFound()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appt = CreateSeriesAppointment(futureDate);
        appt.Cancel("Already cancelled");

        _context.Appointments.Add(appt);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new CancelAppointmentSeriesCommand(SeriesId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
