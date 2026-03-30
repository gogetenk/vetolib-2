using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.SuggestSlot;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class SuggestSlotHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId1 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid VetId2 = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AnimalId = new("44444444-4444-4444-4444-444444444444");

    private static readonly DateOnly TestDate = new(2026, 6, 15); // future date, fixed
    private static readonly TimeOnly PreferredTime = new(10, 0);
    private const string ConsultationType = "checkup";

    private readonly AgendaDbContext _context;
    private readonly SlotScoringService _scoringService;
    private readonly DurationEstimator _durationEstimator;
    private readonly SuggestSlotHandler _handler;

    public SuggestSlotHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var dbOptions = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(dbOptions, clinicContext, publisher);

        // Use real services with default options (working hours 09:00-18:00, weights sum to 1.0)
        var agendaOptions = Options.Create(new AgendaOptions());
        _scoringService = new SlotScoringService(agendaOptions);
        _durationEstimator = new DurationEstimator(_context, agendaOptions);

        _handler = new SuggestSlotHandler(_context, _scoringService, _durationEstimator);
    }

    private Appointment SeedAppointment(
        Guid vetId,
        string vetName,
        TimeOnly startTime,
        int durationMinutes = 30,
        string reason = "checkup",
        AppointmentStatus? statusOverride = null)
    {
        var appt = Appointment.Create(
            ClinicId, vetId, vetName,
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            TestDate, startTime, durationMinutes, reason).Value;

        if (statusOverride == AppointmentStatus.Completed)
        {
            appt.CheckIn();
            appt.StartConsultation();
            appt.Complete();
        }
        else if (statusOverride == AppointmentStatus.Cancelled)
        {
            appt.Cancel();
        }

        _context.Appointments.Add(appt);
        return appt;
    }

    // ── Happy Path: returns top 3 slots ──────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAppointmentsExist_ReturnsSuccessWithUpTo3Suggestions()
    {
        // Vet1 has one appointment at 10:00
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0));
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: null,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().HaveCountLessOrEqualTo(3);
        result.Value.Suggestions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenMultipleVetsScoredAcrossDay_ReturnsTop3ByScore()
    {
        // Vet1 with several appointments
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(9, 0));
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(9, 30));
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0));

        // Vet2 with fewer appointments
        SeedAppointment(VetId2, "Dr. Sara Al-Zaabi", new TimeOnly(11, 0));
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: null,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Count.Should().BeLessOrEqualTo(3);

        // Suggestions must be in descending score order
        var scores = result.Value.Suggestions.Select(s => s.Score).ToList();
        scores.Should().BeInDescendingOrder();
    }

    // ── With preferred vet: only considers that vet ───────────────────────────────

    [Fact]
    public async Task Handle_WhenPreferredVetSpecified_OnlyReturnsSlotsForThatVet()
    {
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0));
        SeedAppointment(VetId2, "Dr. Sara Al-Zaabi", new TimeOnly(11, 0));
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.VeterinarianId.Should().Be(VetId1));
    }

    [Fact]
    public async Task Handle_WhenPreferredVetSpecified_AllSuggestionsBelongToThatVet()
    {
        // Multiple vets in DB but query filters to VetId2 only
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(9, 0));
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0));
        SeedAppointment(VetId2, "Dr. Sara Al-Zaabi", new TimeOnly(14, 0));
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: new TimeOnly(14, 0),
            PreferredVeterinarianId: VetId2,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.VeterinarianId.Should().Be(VetId2));
        result.Value.Suggestions.Should().NotContain(s => s.VeterinarianId == VetId1);
    }

    // ── No existing appointments for preferred vet: creates empty schedule ────────

    [Fact]
    public async Task Handle_WhenPreferredVetHasNoAppointments_ReturnsScheduleForThatVet()
    {
        // DB is empty for VetId1 on TestDate
        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // With no appointments the full day is available — should find multiple slots
        result.Value.Suggestions.Should().NotBeEmpty();
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.VeterinarianId.Should().Be(VetId1));
    }

    [Fact]
    public async Task Handle_WhenPreferredVetHasNoAppointments_VetNameIsDerivedFromId()
    {
        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // When no appointments exist for vet, handler uses $"Vet {vetId:N}" as fallback name
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.VeterinarianName.Should().Contain(VetId1.ToString("N")));
    }

    // ── Empty database: returns empty suggestions ─────────────────────────────────

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_AndNoPreferredVet_ReturnsEmptySuggestions()
    {
        // No appointments, no preferred vet — no vets to consider
        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: null,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().BeEmpty();
    }

    // ── Cancelled/NoShow appointments are excluded from schedule ──────────────────

    [Fact]
    public async Task Handle_WhenCancelledAppointmentsExist_TheyAreNotCountedAsOccupied()
    {
        // Cancelled appointment — should NOT appear as a loaded appointment
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0),
            statusOverride: AppointmentStatus.Cancelled);
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: new TimeOnly(10, 0), // same slot as cancelled appt
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // If cancelled appt is correctly excluded, the 10:00 slot should be available
        result.Value.Suggestions.Should().Contain(s => s.StartTime == new TimeOnly(10, 0));
    }

    // ── DurationMinutes override respected ────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDurationMinutesProvided_SlotsHaveCorrectDuration()
    {
        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 45);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.EstimatedDurationMinutes.Should().Be(45));
    }

    [Fact]
    public async Task Handle_WhenDurationMinutesNull_FallsBackToEstimatedDuration()
    {
        // No history → DurationEstimator returns default (30 min)
        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Suggestions.Should().AllSatisfy(s =>
            s.EstimatedDurationMinutes.Should().BePositive());
    }

    // ── Score ordering ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ReturnedSuggestions_AreOrderedByScoreDescending()
    {
        SeedAppointment(VetId1, "Dr. Khalid Al-Mansouri", new TimeOnly(10, 0));
        await _context.SaveChangesAsync();

        var query = new SuggestSlotQuery(
            ConsultationType: ConsultationType,
            PreferredDate: TestDate,
            PreferredTime: PreferredTime,
            PreferredVeterinarianId: VetId1,
            DurationMinutes: 30);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var scores = result.Value.Suggestions.Select(s => s.Score).ToList();
        scores.Should().BeInDescendingOrder("suggestions must be ordered by score descending");
    }

    public void Dispose() => _context.Dispose();
}
