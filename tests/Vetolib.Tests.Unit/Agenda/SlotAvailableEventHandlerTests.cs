using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.NotifyWaitlist;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class SlotAvailableEventHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid VetId = new("22222222-2222-2222-2222-222222222222");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private readonly AgendaDbContext _context;
    private readonly SlotAvailableEventHandler _handler;

    public SlotAvailableEventHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();
        var logger = new NullLogger<SlotAvailableEventHandler>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new SlotAvailableEventHandler(_context, logger);
    }

    private async Task<WaitlistEntry> SeedEntry(
        PreferredTimeSlot timeSlot = PreferredTimeSlot.Morning,
        Guid? vetPreference = null,
        DateOnly? preferredDate = null)
    {
        var entry = WaitlistEntry.Create(
            ClinicId, PatientId,
            "Ahmed Al-Rashidi", "+971501234567", "ahmed@email.ae",
            preferredDate ?? FutureDate, timeSlot, vetPreference, "Checkup").Value;

        _context.WaitlistEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task Handle_MatchingPendingEntry_NotifiesEntry()
    {
        var entry = await SeedEntry(PreferredTimeSlot.Morning);

        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Notified);
        updated.NotifiedAt.Should().NotBeNull();
        updated.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_EntryWithAnyTimeSlot_MatchesAnyTime()
    {
        var entry = await SeedEntry(PreferredTimeSlot.Any);

        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(15, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Notified);
    }

    [Fact]
    public async Task Handle_MismatchedDate_DoesNotNotify()
    {
        var entry = await SeedEntry(preferredDate: FutureDate.AddDays(1));

        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Pending);
    }

    [Fact]
    public async Task Handle_MismatchedTimeSlot_DoesNotNotify()
    {
        var entry = await SeedEntry(PreferredTimeSlot.Evening);

        // Morning slot (9:00) should not match Evening preference
        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Pending);
    }

    [Fact]
    public async Task Handle_VetPreferenceMismatch_DoesNotNotify()
    {
        var preferredVet = Guid.NewGuid();
        var entry = await SeedEntry(vetPreference: preferredVet);

        // Different vet than preferred
        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Pending);
    }

    [Fact]
    public async Task Handle_VetPreferenceNull_MatchesAnyVet()
    {
        var entry = await SeedEntry(vetPreference: null);

        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);
        await _handler.Handle(evt, CancellationToken.None);

        var updated = await _context.WaitlistEntries.FirstAsync();
        updated.Status.Should().Be(WaitlistEntryStatus.Notified);
    }

    [Fact]
    public async Task Handle_NoMatchingEntries_DoesNothing()
    {
        // No entries seeded
        var evt = new SlotAvailableEvent(ClinicId, VetId, FutureDate, new TimeOnly(9, 0), 30);

        // Should not throw
        await _handler.Handle(evt, CancellationToken.None);
    }

    public void Dispose() => _context.Dispose();
}
