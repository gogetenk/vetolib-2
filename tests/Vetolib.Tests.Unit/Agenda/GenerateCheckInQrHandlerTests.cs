using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.GenerateCheckInQr;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class GenerateCheckInQrHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
    private static readonly TimeOnly StartTime = new(10, 0);

    private readonly AgendaDbContext _context;
    private readonly GenerateCheckInQrHandler _handler;

    public GenerateCheckInQrHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CheckIn:HmacKey"] = "test-hmac-key-at-least-32-chars-long!"
            })
            .Build();

        var hmacService = new CheckInHmacService(config);
        _handler = new GenerateCheckInQrHandler(_context, hmacService);
    }

    private async Task<Appointment> SeedAppointment(AppointmentStatus? advanceTo = null)
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, StartTime, 30, "Annual checkup").Value;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        if (advanceTo == AppointmentStatus.CheckedIn)
        {
            appointment.CheckIn();
            await _context.SaveChangesAsync();
        }

        return appointment;
    }

    [Fact]
    public async Task Handle_ScheduledAppointment_ReturnsSignedPayload()
    {
        var appointment = await SeedAppointment();

        var result = await _handler.Handle(
            new GenerateCheckInQrCommand(appointment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AppointmentId.Should().Be(appointment.Id);
        result.Value.PatientName.Should().Be("Luna");
        result.Value.OwnerName.Should().Be("Ahmed Al-Rashidi");
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.Signature.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new GenerateCheckInQrCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_NonScheduledAppointment_ReturnsError()
    {
        var appointment = await SeedAppointment(AppointmentStatus.CheckedIn);

        var result = await _handler.Handle(
            new GenerateCheckInQrCommand(appointment.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().Contain(e => e.Contains("INVALID_STATUS"));
    }

    public void Dispose() => _context.Dispose();
}
