using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.SubmitVisitFeedback;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class SubmitVisitFeedbackHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private readonly AgendaDbContext _context;
    private readonly IClinicContext _clinicContext;
    private readonly SubmitVisitFeedbackHandler _handler;

    public SubmitVisitFeedbackHandlerTests()
    {
        _clinicContext = Substitute.For<IClinicContext>();
        _clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, _clinicContext, publisher);
        _handler = new SubmitVisitFeedbackHandler(_context, _clinicContext);
    }

    private async Task<Appointment> SeedCompletedAppointment()
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", "ahmed@email.ae",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            new TimeOnly(10, 0), 30, "Checkup").Value;

        // Transition to Completed: Scheduled -> CheckedIn -> InProgress -> Completed
        appointment.CheckIn();
        appointment.StartConsultation();
        appointment.Complete();

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    private SubmitVisitFeedbackCommand BuildCommand(Guid appointmentId, int rating = 5, string? comment = "Great!", bool isPublic = true)
        => new(appointmentId, rating, comment, isPublic);

    [Fact]
    public async Task Handle_WhenValidFeedbackOnCompletedAppointment_ReturnsSuccess()
    {
        var appointment = await SeedCompletedAppointment();
        var cmd = BuildCommand(appointment.Id);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AppointmentId.Should().Be(appointment.Id);
        result.Value.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Great!");
        result.Value.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotFound_ReturnsNotFound()
    {
        var cmd = BuildCommand(Guid.NewGuid());

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAppointmentNotCompleted_ReturnsError()
    {
        // Seed a Scheduled appointment (not completed)
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            new TimeOnly(10, 0), 30, "Checkup").Value;
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var cmd = BuildCommand(appointment.Id);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("APPOINTMENT_NOT_COMPLETED"));
    }

    [Fact]
    public async Task Handle_WhenFeedbackAlreadyExists_ReturnsError()
    {
        var appointment = await SeedCompletedAppointment();

        // Submit first feedback
        var cmd1 = BuildCommand(appointment.Id);
        await _handler.Handle(cmd1, CancellationToken.None);

        // Try to submit a second feedback
        var cmd2 = BuildCommand(appointment.Id, rating: 3, comment: "Changed my mind");

        var result = await _handler.Handle(cmd2, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("FEEDBACK_ALREADY_EXISTS"));
    }

    [Fact]
    public async Task Handle_WhenValidFeedback_PersistsToDatabase()
    {
        var appointment = await SeedCompletedAppointment();
        var cmd = BuildCommand(appointment.Id);

        await _handler.Handle(cmd, CancellationToken.None);

        var saved = await _context.VisitFeedbacks.CountAsync();
        saved.Should().Be(1);
    }

    public void Dispose() => _context.Dispose();
}
