using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateAppointment;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateAppointmentHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    // Business hours: 09:00-18:00. Use a date in the future.
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly ValidStart = new(10, 0);
    private const int ValidDuration = 30;

    private readonly AgendaDbContext _context;
    private readonly IPublisher _publisher;
    private readonly CreateAppointmentHandler _handler;

    public CreateAppointmentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, _publisher);
        _handler = new CreateAppointmentHandler(_context);
    }

    private CreateAppointmentCommand BuildCommand(
        DateOnly? date = null,
        TimeOnly? startTime = null,
        int? durationMinutes = null,
        Guid? vetId = null) =>
        new(
            ClinicId: ClinicId,
            VeterinarianId: vetId ?? VetId,
            VeterinarianName: "Dr. Khalid Al-Mansouri",
            AnimalId: AnimalId,
            AnimalName: "Luna",
            OwnerName: "Ahmed Al-Rashidi",
            OwnerEmail: "ahmed@email.ae",
            Date: date ?? FutureDate,
            StartTime: startTime ?? ValidStart,
            DurationMinutes: durationMinutes ?? ValidDuration,
            Reason: "Annual checkup");

    // ── Happy Path ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccess()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.AnimalName.Should().Be("Luna");
        result.Value.OwnerName.Should().Be("Ahmed Al-Rashidi");
        result.Value.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_PersistsAppointmentToDatabase()
    {
        var cmd = BuildCommand();

        await _handler.Handle(cmd, CancellationToken.None);

        var saved = await _context.Appointments.CountAsync();
        saved.Should().Be(1);
    }

    // ── Past Date ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDateIsInPast_ReturnsError()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var cmd = BuildCommand(date: pastDate);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("PAST_DATE_NOT_ALLOWED"));
    }

    // ── Outside Business Hours ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenStartTimeBeforeOpeningHours_ReturnsError()
    {
        var earlyStart = new TimeOnly(7, 0); // before 09:00
        var cmd = BuildCommand(startTime: earlyStart);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("OUTSIDE_BUSINESS_HOURS"));
    }

    [Fact]
    public async Task Handle_WhenEndTimeAfterClosingHours_ReturnsError()
    {
        var lateStart = new TimeOnly(17, 45); // ends at 18:15, after 18:00
        var cmd = BuildCommand(startTime: lateStart, durationMinutes: 30);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("OUTSIDE_BUSINESS_HOURS"));
    }

    // ── Conflict Detection ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenSlotAlreadyTakenForSameVet_ReturnsConflictError()
    {
        // Seed an existing appointment for the same vet at the same time
        var existing = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, ValidStart, ValidDuration, "First booking").Value;
        _context.Appointments.Add(existing);
        await _context.SaveChangesAsync();

        // Attempt to book the same vet at the same overlapping time
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("APPOINTMENT_CONFLICT"));
    }

    [Fact]
    public async Task Handle_WhenSlotTakenForSameVetButCancelled_AllowsBooking()
    {
        // Seed a CANCELLED appointment — should not block new bookings
        var existing = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, ValidStart, ValidDuration, "Cancelled").Value;
        existing.Cancel("No longer needed");
        _context.Appointments.Add(existing);
        await _context.SaveChangesAsync();

        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSlotTakenForDifferentVet_AllowsBooking()
    {
        var otherVetId = Guid.NewGuid();

        // Seed existing appointment for a DIFFERENT vet
        var existing = Appointment.Create(
            ClinicId, otherVetId, "Dr. Sara Al-Zaabi",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, ValidStart, ValidDuration, "Other vet").Value;
        _context.Appointments.Add(existing);
        await _context.SaveChangesAsync();

        // Book same time but for VetId (different vet)
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose() => _context.Dispose();
}
