using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CheckInFromQr;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Services;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CheckInFromQrHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");
    private static readonly DateOnly FutureDate = new(2026, 4, 1);
    private static readonly TimeOnly StartTime = new(10, 0);
    private static readonly DateTime ScheduledDateTime = FutureDate.ToDateTime(StartTime, DateTimeKind.Utc);

    private readonly AgendaDbContext _context;
    private readonly CheckInHmacService _hmacService;
    private readonly FakeTimeProvider _timeProvider;
    private readonly CheckInFromQrHandler _handler;

    public CheckInFromQrHandlerTests()
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

        _hmacService = new CheckInHmacService(config);
        _timeProvider = new FakeTimeProvider(ScheduledDateTime);
        _handler = new CheckInFromQrHandler(_context, _hmacService, _timeProvider);
    }

    private async Task<Appointment> SeedAppointment()
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", null,
            FutureDate, StartTime, 30, "Annual checkup").Value;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    private CheckInFromQrCommand BuildCommand(Appointment appointment, string? signatureOverride = null)
    {
        var sig = signatureOverride ?? _hmacService.ComputeSignature(
            appointment.Id, "Luna", "Ahmed Al-Rashidi", ScheduledDateTime, ClinicId);

        return new CheckInFromQrCommand(
            appointment.Id, "Luna", "Ahmed Al-Rashidi", ScheduledDateTime, ClinicId, sig);
    }

    [Fact]
    public async Task Handle_ValidPayload_WithinWindow_ChecksInSuccessfully()
    {
        var appointment = await SeedAppointment();
        var cmd = BuildCommand(appointment);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    [Fact]
    public async Task Handle_InvalidSignature_ReturnsError()
    {
        var appointment = await SeedAppointment();
        var cmd = BuildCommand(appointment, signatureOverride: "tampered-signature");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_SIGNATURE"));
    }

    [Fact]
    public async Task Handle_OutsideTimeWindow_ReturnsError()
    {
        var appointment = await SeedAppointment();
        _timeProvider.SetUtcNow(ScheduledDateTime.AddMinutes(31));
        var cmd = BuildCommand(appointment);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("CHECKIN_WINDOW_EXPIRED"));
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var sig = _hmacService.ComputeSignature(
            nonExistentId, "Luna", "Ahmed Al-Rashidi", ScheduledDateTime, ClinicId);

        var cmd = new CheckInFromQrCommand(
            nonExistentId, "Luna", "Ahmed Al-Rashidi", ScheduledDateTime, ClinicId, sig);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_AlreadyCheckedIn_ReturnsError()
    {
        var appointment = await SeedAppointment();
        appointment.CheckIn(); // advance to CheckedIn
        await _context.SaveChangesAsync();

        var cmd = BuildCommand(appointment);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INVALID_TRANSITION"));
    }

    public void Dispose() => _context.Dispose();

    /// <summary>
    /// Simple fake TimeProvider for testing time-dependent behavior.
    /// </summary>
    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTime utcNow) => _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

        public void SetUtcNow(DateTime utcNow) => _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
