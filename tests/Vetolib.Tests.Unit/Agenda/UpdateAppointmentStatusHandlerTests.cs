using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.UpdateAppointmentStatus;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class UpdateAppointmentStatusHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly StartTime = new(10, 0);

    private readonly AgendaDbContext _context;
    private readonly IPublisher _publisher;
    private readonly UpdateAppointmentStatusHandler _handler;

    public UpdateAppointmentStatusHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, _publisher);
        _handler = new UpdateAppointmentStatusHandler(_context);
    }

    private async Task<Appointment> SeedAppointment(AppointmentStatus? advanceTo = null)
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, StartTime, 30, "Annual checkup").Value;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Advance status if requested (bypassing the handler)
        if (advanceTo.HasValue)
        {
            switch (advanceTo.Value)
            {
                case AppointmentStatus.CheckedIn:
                    appointment.CheckIn();
                    break;
                case AppointmentStatus.InProgress:
                    appointment.CheckIn();
                    appointment.StartConsultation();
                    break;
                case AppointmentStatus.Completed:
                    appointment.CheckIn();
                    appointment.StartConsultation();
                    appointment.Complete();
                    break;
                case AppointmentStatus.Cancelled:
                    appointment.Cancel();
                    break;
            }
            await _context.SaveChangesAsync();
        }

        return appointment;
    }

    // ── Appointment Not Found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: Guid.NewGuid(),
            NewStatus: AppointmentStatus.CheckedIn,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── Valid Transitions ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ScheduledToCheckedIn_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(); // Scheduled by default

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.CheckedIn,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    [Fact]
    public async Task Handle_CheckedInToInProgress_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(AppointmentStatus.CheckedIn);

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.InProgress,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public async Task Handle_InProgressToCompleted_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(AppointmentStatus.InProgress);

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.Completed,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task Handle_ScheduledToCancelled_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(); // Scheduled

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.Cancelled,
            Reason: "Owner requested cancellation");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    // ── Invalid Transitions ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_CompletedToScheduled_ReturnsError_InvalidTransition()
    {
        var appointment = await SeedAppointment(AppointmentStatus.Completed);

        // Attempting to transition to CheckedIn from Completed (invalid)
        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.CheckedIn,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public async Task Handle_CancelledToCancelled_ReturnsError_InvalidTransition()
    {
        var appointment = await SeedAppointment(AppointmentStatus.Cancelled);

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.Cancelled,
            Reason: "Trying again");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    [Fact]
    public async Task Handle_ScheduledToInProgress_SkipsCheckedIn_ReturnsError()
    {
        var appointment = await SeedAppointment(); // Scheduled

        // StartConsultation requires CheckedIn first
        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.InProgress,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    // ── NoShow ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ScheduledToNoShow_ReturnsSuccess()
    {
        var appointment = await SeedAppointment(); // Scheduled

        var cmd = new UpdateAppointmentStatusCommand(
            AppointmentId: appointment.Id,
            NewStatus: AppointmentStatus.NoShow,
            Reason: null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.NoShow);
    }

    public void Dispose() => _context.Dispose();
}
