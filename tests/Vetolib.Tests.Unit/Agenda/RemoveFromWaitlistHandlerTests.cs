using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.RemoveFromWaitlist;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class RemoveFromWaitlistHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("44444444-4444-4444-4444-444444444444");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private readonly AgendaDbContext _context;
    private readonly RemoveFromWaitlistHandler _handler;

    public RemoveFromWaitlistHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new RemoveFromWaitlistHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenEntryExists_RemovesAndReturnsSuccess()
    {
        var entry = WaitlistEntry.Create(
            ClinicId, PatientId,
            "Ahmed Al-Rashidi", "+971501234567", null,
            FutureDate, PreferredTimeSlot.Morning, null, null).Value;

        _context.WaitlistEntries.Add(entry);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new RemoveFromWaitlistCommand(entry.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _context.WaitlistEntries.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEntryNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new RemoveFromWaitlistCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
