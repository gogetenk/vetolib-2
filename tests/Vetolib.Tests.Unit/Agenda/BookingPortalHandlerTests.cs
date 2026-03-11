using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateBookingAppointment;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.GetOwnerAppointmentById;
using Vetolib.Agenda.Application.Queries.ListOwnerAppointments;
using Vetolib.Agenda.Application.Queries.ListPublicConsultationTypes;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

/// <summary>
/// Unit tests for the owner booking portal CQRS handlers.
/// Tests: CreateBookingAppointmentHandler, ListOwnerAppointmentsHandler,
///        GetOwnerAppointmentByIdHandler, ListPublicConsultationTypesHandler.
/// </summary>
public class BookingPortalHandlerTests : IDisposable
{
    // Fixed GUIDs — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid VetId = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AnimalId = new("44444444-4444-4444-4444-444444444444");

    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly ValidStart = new(10, 0);
    private const int ValidDuration = 30;

    private readonly AgendaDbContext _context;
    private readonly IPublisher _publisher;

    public BookingPortalHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);
        _publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, _publisher);
    }

    // ── CreateBookingAppointmentHandler ──────────────────────────────────────────

    [Fact]
    public async Task CreateBooking_WhenValidCommand_CreatesAppointmentWithOwnerPortalSource()
    {
        var handler = new CreateBookingAppointmentHandler(_context);
        var cmd = BuildCreateCommand();

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be(BookingSource.OwnerPortal);
        result.Value.Status.Should().Be(AppointmentStatus.Scheduled);
        result.Value.AnimalName.Should().Be("Simba");
    }

    [Fact]
    public async Task CreateBooking_WhenDateInPast_ReturnsError()
    {
        var handler = new CreateBookingAppointmentHandler(_context);
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var cmd = BuildCreateCommand(date: pastDate);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("PAST_DATE_NOT_ALLOWED"));
    }

    [Fact]
    public async Task CreateBooking_WhenDateBeyondMaxAdvanceDays_ReturnsError()
    {
        var handler = new CreateBookingAppointmentHandler(_context);
        // MaxAdvanceDays is 90 — try 91 days ahead
        var tooFarDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(91));
        var cmd = BuildCreateCommand(date: tooFarDate);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("DATE_TOO_FAR"));
    }

    [Fact]
    public async Task CreateBooking_WhenSlotConflict_ReturnsConflictError()
    {
        // Seed an existing appointment at the same time
        var existing = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Luna", "Mohammed Al-Rashidi",
            FutureDate, ValidStart, ValidDuration, "Existing",
            source: BookingSource.Staff).Value;
        _context.Appointments.Add(existing);
        await _context.SaveChangesAsync();

        var handler = new CreateBookingAppointmentHandler(_context);
        var cmd = BuildCreateCommand();

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("APPOINTMENT_CONFLICT"));
    }

    // ── ListOwnerAppointmentsHandler ─────────────────────────────────────────────

    [Fact]
    public async Task ListOwnerAppointments_WhenOwnerHasAppointments_ReturnsThemSortedByDateDesc()
    {
        // Seed two appointments for OwnerId
        var appt1 = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Simba", "Aisha Al-Mansouri",
            FutureDate, ValidStart, ValidDuration, "Checkup",
            source: BookingSource.OwnerPortal, ownerId: OwnerId).Value;

        var appt2 = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Simba", "Aisha Al-Mansouri",
            FutureDate.AddDays(2), ValidStart, ValidDuration, "Follow-up",
            source: BookingSource.OwnerPortal, ownerId: OwnerId).Value;

        _context.Appointments.AddRange(appt1, appt2);
        await _context.SaveChangesAsync();

        var handler = new ListOwnerAppointmentsHandler(_context);
        var result = await handler.Handle(
            new ListOwnerAppointmentsQuery(OwnerId, ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        // Sorted descending by date — the further-future date should come first
        result.Value[0].Date.Should().BeAfter(result.Value[1].Date);
    }

    [Fact]
    public async Task ListOwnerAppointments_DoesNotReturnOtherOwnersAppointments()
    {
        var otherOwnerId = Guid.NewGuid();

        // Seed appointment for a different owner
        var appt = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Max", "Khalid Al-Rashidi",
            FutureDate, ValidStart, ValidDuration, null,
            source: BookingSource.OwnerPortal, ownerId: otherOwnerId).Value;
        _context.Appointments.Add(appt);
        await _context.SaveChangesAsync();

        var handler = new ListOwnerAppointmentsHandler(_context);
        var result = await handler.Handle(
            new ListOwnerAppointmentsQuery(OwnerId, ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── GetOwnerAppointmentByIdHandler ───────────────────────────────────────────

    [Fact]
    public async Task GetOwnerAppointmentById_WhenBelongsToOwner_ReturnsSuccess()
    {
        var appt = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Simba", "Aisha Al-Mansouri",
            FutureDate, ValidStart, ValidDuration, null,
            source: BookingSource.OwnerPortal, ownerId: OwnerId).Value;
        _context.Appointments.Add(appt);
        await _context.SaveChangesAsync();

        var handler = new GetOwnerAppointmentByIdHandler(_context);
        var result = await handler.Handle(
            new GetOwnerAppointmentByIdQuery(appt.Id, OwnerId, ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(appt.Id);
    }

    [Fact]
    public async Task GetOwnerAppointmentById_WhenBelongsToDifferentOwner_ReturnsForbidden()
    {
        var otherOwnerId = Guid.NewGuid();

        var appt = Appointment.Create(
            ClinicId, VetId, "Dr. Fatima Al-Zahrawi",
            AnimalId, "Max", "Khalid Al-Rashidi",
            FutureDate, ValidStart, ValidDuration, null,
            source: BookingSource.OwnerPortal, ownerId: otherOwnerId).Value;
        _context.Appointments.Add(appt);
        await _context.SaveChangesAsync();

        var handler = new GetOwnerAppointmentByIdHandler(_context);
        var result = await handler.Handle(
            new GetOwnerAppointmentByIdQuery(appt.Id, OwnerId, ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public async Task GetOwnerAppointmentById_WhenNotFound_ReturnsNotFound()
    {
        var handler = new GetOwnerAppointmentByIdHandler(_context);
        var result = await handler.Handle(
            new GetOwnerAppointmentByIdQuery(Guid.NewGuid(), OwnerId, ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── ListPublicConsultationTypesHandler ────────────────────────────────────────

    [Fact]
    public async Task ListPublicConsultationTypes_WhenClinicHasActiveTypes_ReturnsThem()
    {
        var type1Result = ConsultationType.Create(ClinicId, "Annual Checkup", 30, 1);
        var type2Result = ConsultationType.Create(ClinicId, "Vaccination", 15, 2);
        _context.ConsultationTypes.AddRange(type1Result.Value, type2Result.Value);
        await _context.SaveChangesAsync();

        var handler = new ListPublicConsultationTypesHandler(_context);
        var result = await handler.Handle(
            new ListPublicConsultationTypesQuery(ClinicId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.All(t => t.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task ListPublicConsultationTypes_WhenEmptyClinicId_ReturnsError()
    {
        var handler = new ListPublicConsultationTypesHandler(_context);
        var result = await handler.Handle(
            new ListPublicConsultationTypesQuery(Guid.Empty), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_CLINIC_ID"));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static CreateBookingAppointmentCommand BuildCreateCommand(DateOnly? date = null) =>
        new(
            ClinicId: ClinicId,
            OwnerId: OwnerId,
            VeterinarianId: VetId,
            VeterinarianName: "Dr. Fatima Al-Zahrawi",
            AnimalId: AnimalId,
            AnimalName: "Simba",
            OwnerName: "Aisha Al-Mansouri",
            Date: date ?? FutureDate,
            StartTime: ValidStart,
            DurationMinutes: ValidDuration,
            Reason: "Annual checkup");

    public void Dispose() => _context.Dispose();
}
