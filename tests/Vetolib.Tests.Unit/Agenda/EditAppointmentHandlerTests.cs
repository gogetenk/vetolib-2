using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.EditAppointment;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class EditAppointmentHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly StartTime = new(10, 0);

    private readonly AgendaDbContext _context;
    private readonly IPublisher _publisher;
    private readonly EditAppointmentHandler _handler;

    public EditAppointmentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, _publisher);
        _handler = new EditAppointmentHandler(_context, cache: null);
    }

    private async Task<Appointment> SeedAppointment(
        TimeOnly? startTime = null,
        int durationMinutes = 30,
        Guid? vetId = null,
        DateOnly? date = null)
    {
        var appointment = Appointment.Create(
            ClinicId,
            vetId ?? VetId,
            "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi",
            date ?? FutureDate,
            startTime ?? StartTime,
            durationMinutes,
            "Annual checkup").Value;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    // ── Happy Path ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAppointmentExists_UpdatesAndReturnsSuccess()
    {
        var appointment = await SeedAppointment();
        var newDate = FutureDate.AddDays(1);

        var cmd = new EditAppointmentCommand(
            AppointmentId: appointment.Id,
            Date: newDate,
            StartTime: new TimeOnly(11, 0),
            DurationMinutes: 45,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: "Follow-up visit",
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Should().Be(newDate);
        result.Value.StartTime.Should().Be(new TimeOnly(11, 0));
        result.Value.DurationMinutes.Should().Be(45);
        result.Value.Reason.Should().Be("Follow-up visit");
    }

    [Fact]
    public async Task Handle_WhenOnlyReasonChanged_ReturnSuccessWithUpdatedReason()
    {
        var appointment = await SeedAppointment();

        var cmd = new EditAppointmentCommand(
            AppointmentId: appointment.Id,
            Date: null,
            StartTime: null,
            DurationMinutes: null,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: "Updated reason",
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Reason.Should().Be("Updated reason");
    }

    // ── Appointment Not Found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = new EditAppointmentCommand(
            AppointmentId: Guid.NewGuid(), // does not exist
            Date: FutureDate,
            StartTime: new TimeOnly(11, 0),
            DurationMinutes: 30,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: null,
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── Conflict After Edit ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenRescheduledSlotConflictsWithAnotherAppointment_ReturnsError()
    {
        // Seed target appointment at 10:00
        var target = await SeedAppointment(startTime: new TimeOnly(10, 0));

        // Seed blocking appointment for same vet at 11:00
        await SeedAppointment(startTime: new TimeOnly(11, 0));

        // Try to move target to 11:00 — conflicts with blocking appointment
        var cmd = new EditAppointmentCommand(
            AppointmentId: target.Id,
            Date: FutureDate,
            StartTime: new TimeOnly(11, 0),
            DurationMinutes: 30,
            VeterinarianId: VetId,
            VeterinarianName: "Dr. Khalid Al-Mansouri",
            Reason: null,
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("APPOINTMENT_CONFLICT"));
    }

    // ── Invalid Domain Transition ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAppointmentIsCompleted_ReturnsError()
    {
        var appointment = await SeedAppointment();

        // Advance to Completed via domain methods
        appointment.CheckIn();
        appointment.StartConsultation();
        appointment.Complete();
        await _context.SaveChangesAsync();

        var cmd = new EditAppointmentCommand(
            AppointmentId: appointment.Id,
            Date: FutureDate.AddDays(1),
            StartTime: null,
            DurationMinutes: null,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: null,
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public async Task Handle_WhenAppointmentIsCancelled_ReturnsError()
    {
        var appointment = await SeedAppointment();
        appointment.Cancel("Test cancel");
        await _context.SaveChangesAsync();

        var cmd = new EditAppointmentCommand(
            AppointmentId: appointment.Id,
            Date: FutureDate.AddDays(1),
            StartTime: null,
            DurationMinutes: null,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: null,
            Notes: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    public void Dispose() => _context.Dispose();
}
