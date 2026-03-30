using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.GetConsultationBreakdown;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class GetConsultationBreakdownHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private readonly AgendaDbContext _context;
    private readonly GetConsultationBreakdownHandler _handler;

    public GetConsultationBreakdownHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new GetConsultationBreakdownHandler(_context);
    }

    [Fact]
    public async Task Handle_NoAppointments_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetConsultationBreakdownQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AppointmentsThisMonth_GroupsByReason()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, "Vaccination");
        await SeedAppointment(today, "Vaccination");
        await SeedAppointment(today, "General Checkup");

        var result = await _handler.Handle(new GetConsultationBreakdownQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First(x => x.ConsultationType == "Vaccination").Count.Should().Be(2);
        result.Value.First(x => x.ConsultationType == "General Checkup").Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AppointmentsLastMonth_AreExcluded()
    {
        var lastMonth = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(lastMonth, "Surgery");
        await SeedAppointment(today, "Vaccination");

        var result = await _handler.Handle(new GetConsultationBreakdownQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ConsultationType.Should().Be("Vaccination");
    }

    [Fact]
    public async Task Handle_NullReason_GroupedAsUnspecified()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, null);

        var result = await _handler.Handle(new GetConsultationBreakdownQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].ConsultationType.Should().Be("Unspecified");
    }

    [Fact]
    public async Task Handle_OrderedByCountDescending()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, "Surgery");
        await SeedAppointment(today, "Vaccination");
        await SeedAppointment(today, "Vaccination");
        await SeedAppointment(today, "Vaccination");
        await SeedAppointment(today, "General Checkup");
        await SeedAppointment(today, "General Checkup");

        var result = await _handler.Handle(new GetConsultationBreakdownQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].ConsultationType.Should().Be("Vaccination");
        result.Value[1].ConsultationType.Should().Be("General Checkup");
        result.Value[2].ConsultationType.Should().Be("Surgery");
    }

    private async Task SeedAppointment(DateOnly date, string? reason)
    {
        var appointment = Appointment.Create(
            ClinicId, VetId, "Dr. Khalid Al-Mansouri",
            AnimalId, "Luna", "Ahmed Al-Rashidi", "ahmed@email.ae",
            date, new TimeOnly(10, 0), 30, reason);

        _context.Appointments.Add(appointment.Value);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
