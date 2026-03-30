using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.AddToWaitlist;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class AddToWaitlistHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("44444444-4444-4444-4444-444444444444");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private readonly AgendaDbContext _context;
    private readonly AddToWaitlistHandler _handler;

    public AddToWaitlistHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new AddToWaitlistHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessAndPersists()
    {
        var cmd = new AddToWaitlistCommand(
            ClinicId, PatientId,
            "Fatima Al-Zahra", "+971501234567", "fatima@email.ae",
            FutureDate, PreferredTimeSlot.Morning, null, "Dental cleaning");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WaitlistEntryStatus.Pending);
        result.Value.OwnerName.Should().Be("Fatima Al-Zahra");

        var persisted = await _context.WaitlistEntries.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyPatientId_ReturnsInvalid()
    {
        var cmd = new AddToWaitlistCommand(
            ClinicId, Guid.Empty,
            "Fatima Al-Zahra", "+971501234567", null,
            FutureDate, PreferredTimeSlot.Any, null, null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithVetPreference_PersistsVetId()
    {
        var vetId = Guid.NewGuid();
        var cmd = new AddToWaitlistCommand(
            ClinicId, PatientId,
            "Omar Hassan", "+971559876543", "omar@email.ae",
            FutureDate, PreferredTimeSlot.Afternoon, vetId, "Follow-up with Dr. Khalid");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.VetPreference.Should().Be(vetId);
    }

    public void Dispose() => _context.Dispose();
}
