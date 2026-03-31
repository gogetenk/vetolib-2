using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateFollowUpRule;
using Vetolib.Agenda.Application.Commands.ScheduleFollowUp;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class AppointmentCompletedEventHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private readonly AgendaDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AppointmentCompletedEventHandler _handler;
    private readonly CreateFollowUpRuleHandler _createRuleHandler;

    public AppointmentCompletedEventHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new AppointmentCompletedEventHandler(
            _context,
            _publishEndpoint,
            NullLogger<AppointmentCompletedEventHandler>.Instance);
        _createRuleHandler = new CreateFollowUpRuleHandler(_context);
    }

    private async Task SeedRule(string consultationType, int followUpDays, string reason)
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, consultationType, followUpDays, reason);
        var result = await _createRuleHandler.Handle(cmd, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    private AppointmentCompletedEvent CreateEvent(string? reason = "Surgery", string? ownerEmail = "ahmed@example.com")
    {
        return new AppointmentCompletedEvent(
            ClinicId, Guid.NewGuid(), VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", ownerEmail,
            DateOnly.FromDateTime(DateTime.UtcNow), reason);
    }

    [Fact]
    public async Task Handle_MatchingRule_CreatesFollowUpAppointment()
    {
        await SeedRule("Surgery", 10, "Post-surgery follow-up");
        var evt = CreateEvent("Surgery");

        await _handler.Handle(evt, CancellationToken.None);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().HaveCount(1);
        appointments[0].Reason.Should().Be("Post-surgery follow-up");
        appointments[0].Date.Should().Be(evt.Date.AddDays(10));
        appointments[0].VeterinarianId.Should().Be(VetId);
        appointments[0].AnimalId.Should().Be(AnimalId);
        appointments[0].Source.Should().Be(BookingSource.System);
    }

    [Fact]
    public async Task Handle_MatchingRule_PublishesNotification()
    {
        await SeedRule("Surgery", 10, "Post-surgery follow-up");
        var evt = CreateEvent("Surgery", "ahmed@example.com");

        await _handler.Handle(evt, CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<FollowUpScheduledIntegrationEvent>(e =>
                e.OwnerEmail == "ahmed@example.com" &&
                e.PatientName == "Luna" &&
                e.FollowUpReason == "Post-surgery follow-up"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoMatchingRule_DoesNotCreateAppointment()
    {
        await SeedRule("Vaccination", 21, "Vaccination booster check");
        var evt = CreateEvent("Surgery");

        await _handler.Handle(evt, CancellationToken.None);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullReason_SkipsFollowUp()
    {
        await SeedRule("Surgery", 10, "Post-surgery follow-up");
        var evt = CreateEvent(reason: null);

        await _handler.Handle(evt, CancellationToken.None);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyReason_SkipsFollowUp()
    {
        await SeedRule("Surgery", 10, "Post-surgery follow-up");
        var evt = CreateEvent(reason: "  ");

        await _handler.Handle(evt, CancellationToken.None);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoOwnerEmail_DoesNotPublishNotification()
    {
        await SeedRule("Surgery", 10, "Post-surgery follow-up");
        var evt = CreateEvent("Surgery", ownerEmail: null);

        await _handler.Handle(evt, CancellationToken.None);

        // Appointment should still be created
        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().HaveCount(1);

        // But no notification published
        await _publishEndpoint.DidNotReceive().Publish(
            Arg.Any<FollowUpScheduledIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_VaccinationRule_SchedulesAt21Days()
    {
        await SeedRule("Vaccination", 21, "Vaccination booster check");
        var evt = CreateEvent("Vaccination");

        await _handler.Handle(evt, CancellationToken.None);

        var appointments = await _context.Appointments.ToListAsync();
        appointments.Should().HaveCount(1);
        appointments[0].Date.Should().Be(evt.Date.AddDays(21));
        appointments[0].Reason.Should().Be("Vaccination booster check");
    }

    [Fact]
    public async Task Handle_AppointmentCompletedRaisesEventFromDomain()
    {
        // Verify that the Appointment.Complete() method raises AppointmentCompletedEvent
        var appointment = Vetolib.Agenda.Application.Domain.Appointment.Create(
            ClinicId, VetId, "Dr. Khalid", AnimalId, "Luna",
            "Ahmed", "ahmed@example.com",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), 30, "Surgery").Value;

        appointment.CheckIn();
        appointment.StartConsultation();
        appointment.Complete();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentCompletedEvent);
        var completedEvent = (AppointmentCompletedEvent)appointment.DomainEvents.First(e => e is AppointmentCompletedEvent);
        completedEvent.Reason.Should().Be("Surgery");
        completedEvent.VeterinarianId.Should().Be(VetId);
        completedEvent.AnimalId.Should().Be(AnimalId);
    }

    public void Dispose() => _context.Dispose();
}
