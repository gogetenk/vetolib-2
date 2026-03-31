using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.MarkWaitingRoom;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class MarkWaitingRoomHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly TimeOnly StartTime = new(10, 0);

    private readonly AgendaDbContext _context;
    private readonly MarkWaitingRoomHandler _handler;

    public MarkWaitingRoomHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new MarkWaitingRoomHandler(_context);
    }

    private async Task<Appointment> SeedAppointment(DateOnly? date = null)
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            date ?? Today, StartTime, 30, "Annual checkup").Value;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = new MarkWaitingRoomCommand(Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAppointmentIsForToday_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(Today);

        var cmd = new MarkWaitingRoomCommand(appointment.Id);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.WaitingRoom);
    }

    [Fact]
    public async Task Handle_WhenAppointmentIsNotForToday_ReturnsError()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var appointment = await SeedAppointment(tomorrow);

        var cmd = new MarkWaitingRoomCommand(appointment.Id);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_DATE"));
    }

    [Fact]
    public async Task Handle_WhenAppointmentAlreadyCheckedIn_ReturnsError()
    {
        var appointment = await SeedAppointment(Today);
        appointment.CheckIn();
        await _context.SaveChangesAsync();

        var cmd = new MarkWaitingRoomCommand(appointment.Id);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    public void Dispose() => _context.Dispose();
}
