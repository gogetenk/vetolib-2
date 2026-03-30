using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.GetVetWorkload;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class GetVetWorkloadHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Vet1Id = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Vet2Id = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AnimalId = new("33333333-3333-3333-3333-333333333333");

    private readonly AgendaDbContext _context;
    private readonly GetVetWorkloadHandler _handler;

    public GetVetWorkloadHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new GetVetWorkloadHandler(_context);
    }

    [Fact]
    public async Task Handle_NoAppointments_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetVetWorkloadQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AppointmentsThisWeek_GroupsByVet()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, Vet1Id, "Dr. Khalid Al-Mansouri");
        await SeedAppointment(today, Vet1Id, "Dr. Khalid Al-Mansouri");
        await SeedAppointment(today, Vet2Id, "Dr. Sara Al-Maktoum");

        var result = await _handler.Handle(new GetVetWorkloadQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First(x => x.VeterinarianId == Vet1Id).AppointmentCount.Should().Be(2);
        result.Value.First(x => x.VeterinarianId == Vet2Id).AppointmentCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AppointmentsLastWeek_AreExcluded()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastWeek = today.AddDays(-7);

        await SeedAppointment(lastWeek, Vet1Id, "Dr. Khalid Al-Mansouri");
        await SeedAppointment(today, Vet2Id, "Dr. Sara Al-Maktoum");

        var result = await _handler.Handle(new GetVetWorkloadQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].VeterinarianId.Should().Be(Vet2Id);
    }

    [Fact]
    public async Task Handle_OrderedByCountDescending()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, Vet2Id, "Dr. Sara Al-Maktoum");
        await SeedAppointment(today, Vet2Id, "Dr. Sara Al-Maktoum");
        await SeedAppointment(today, Vet2Id, "Dr. Sara Al-Maktoum");
        await SeedAppointment(today, Vet1Id, "Dr. Khalid Al-Mansouri");

        var result = await _handler.Handle(new GetVetWorkloadQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].VeterinarianId.Should().Be(Vet2Id);
        result.Value[0].AppointmentCount.Should().Be(3);
        result.Value[1].VeterinarianId.Should().Be(Vet1Id);
        result.Value[1].AppointmentCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_IncludesVeterinarianName()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await SeedAppointment(today, Vet1Id, "Dr. Khalid Al-Mansouri");

        var result = await _handler.Handle(new GetVetWorkloadQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].VeterinarianName.Should().Be("Dr. Khalid Al-Mansouri");
    }

    private async Task SeedAppointment(DateOnly date, Guid vetId, string vetName)
    {
        var appointment = Appointment.Create(
            ClinicId, vetId, vetName,
            AnimalId, "Luna", "Ahmed Al-Rashidi", "ahmed@email.ae",
            date, new TimeOnly(10, 0), 30, "Checkup");

        _context.Appointments.Add(appointment.Value);
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
