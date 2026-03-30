using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateAppointmentSeries;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateAppointmentSeriesHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly ValidStart = new(10, 0);
    private const int ValidDuration = 30;

    private readonly AgendaDbContext _context;
    private readonly CreateAppointmentSeriesHandler _handler;

    public CreateAppointmentSeriesHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new CreateAppointmentSeriesHandler(_context);
    }

    private CreateAppointmentSeriesCommand BuildCommand(
        DateOnly? startDate = null,
        TimeOnly? startTime = null,
        int? durationMinutes = null,
        RecurrenceFrequency frequency = RecurrenceFrequency.Weekly,
        int count = 4) =>
        new(
            ClinicId: ClinicId,
            VeterinarianId: VetId,
            VeterinarianName: "Dr. Khalid Al-Mansouri",
            AnimalId: AnimalId,
            AnimalName: "Luna",
            OwnerName: "Ahmed Al-Rashidi",
            OwnerEmail: "ahmed@email.ae",
            StartDate: startDate ?? FutureDate,
            StartTime: startTime ?? ValidStart,
            DurationMinutes: durationMinutes ?? ValidDuration,
            Reason: "Weekly wound check",
            Frequency: frequency,
            Count: count);

    // ── Happy Path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidWeeklySeries_CreatesAllAppointments()
    {
        var cmd = BuildCommand(count: 4);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
    }

    [Fact]
    public async Task Handle_WithValidSeries_AllAppointmentsShareSameSeriesId()
    {
        var cmd = BuildCommand(count: 3);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var seriesId = result.Value[0].SeriesId;
        seriesId.Should().NotBeNull();
        result.Value.Should().AllSatisfy(a => a.SeriesId.Should().Be(seriesId));
    }

    [Fact]
    public async Task Handle_WithWeeklyFrequency_DatesAre7DaysApart()
    {
        var cmd = BuildCommand(frequency: RecurrenceFrequency.Weekly, count: 3);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[1].Date.Should().Be(result.Value[0].Date.AddDays(7));
        result.Value[2].Date.Should().Be(result.Value[0].Date.AddDays(14));
    }

    [Fact]
    public async Task Handle_WithValidSeries_PersistsAllToDatabase()
    {
        var cmd = BuildCommand(count: 4);

        await _handler.Handle(cmd, CancellationToken.None);

        var saved = await _context.Appointments.CountAsync();
        saved.Should().Be(4);
    }

    // ── Validation Errors ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithCountBelow2_ReturnsInvalid()
    {
        var cmd = BuildCommand(count: 1);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithPastStartDate_ReturnsError()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var cmd = BuildCommand(startDate: pastDate);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("PAST_DATE_NOT_ALLOWED"));
    }

    [Fact]
    public async Task Handle_WhenOutsideBusinessHours_ReturnsError()
    {
        var cmd = BuildCommand(startTime: new TimeOnly(7, 0));

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("OUTSIDE_BUSINESS_HOURS"));
    }

    // ── Conflict Detection ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenConflictOnOneDate_ReturnsConflictError()
    {
        // Seed an existing appointment on the second occurrence date
        var secondDate = FutureDate.AddDays(7);
        var existing = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            secondDate, ValidStart, ValidDuration, "Existing").Value;
        _context.Appointments.Add(existing);
        await _context.SaveChangesAsync();

        var cmd = BuildCommand(count: 3);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("APPOINTMENT_CONFLICT"));
        result.Errors.Should().Contain(e => e.Contains(secondDate.ToString("yyyy-MM-dd")));
    }

    public void Dispose() => _context.Dispose();
}
