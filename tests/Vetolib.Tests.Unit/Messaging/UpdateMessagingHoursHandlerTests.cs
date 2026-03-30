using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.UpdateMessagingHours;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class UpdateMessagingHoursHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly UpdateMessagingHoursHandler _sut;

    public UpdateMessagingHoursHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new UpdateMessagingHoursHandler(_context, clinicContext);
    }

    [Fact]
    public async Task Handle_WhenNoExistingHours_CreatesNewEntries()
    {
        var days = new List<MessagingHoursDayRequest>
        {
            new(0, new TimeOnly(8, 0), new TimeOnly(17, 0), false), // Sunday
            new(1, new TimeOnly(8, 0), new TimeOnly(17, 0), false), // Monday
        };
        var cmd = new UpdateMessagingHoursCommand(days);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var saved = await _context.MessagingHours.ToListAsync();
        saved.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenExistingHours_UpdatesThem()
    {
        // Seed existing hours for Sunday
        var existing = MessagingHours.Create(ClinicId, 0, new TimeOnly(9, 0), new TimeOnly(18, 0)).Value;
        _context.MessagingHours.Add(existing);
        await _context.SaveChangesAsync();

        var days = new List<MessagingHoursDayRequest>
        {
            new(0, new TimeOnly(7, 0), new TimeOnly(15, 0), false), // Update Sunday
        };
        var cmd = new UpdateMessagingHoursCommand(days);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var saved = await _context.MessagingHours.FirstAsync(h => h.DayOfWeek == 0);
        saved.OpenTime.Should().Be(new TimeOnly(7, 0));
        saved.CloseTime.Should().Be(new TimeOnly(15, 0));
    }

    [Fact]
    public async Task Handle_WithClosedDay_CreatesClosed()
    {
        var days = new List<MessagingHoursDayRequest>
        {
            new(5, new TimeOnly(0, 0), new TimeOnly(0, 0), true), // Friday closed
        };
        var cmd = new UpdateMessagingHoursCommand(days);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var saved = await _context.MessagingHours.FirstAsync(h => h.DayOfWeek == 5);
        saved.IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidDayOfWeek_ReturnsInvalid()
    {
        var days = new List<MessagingHoursDayRequest>
        {
            new(7, new TimeOnly(8, 0), new TimeOnly(17, 0), false), // Invalid day
        };
        var cmd = new UpdateMessagingHoursCommand(days);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithOpenTimeAfterCloseTime_ReturnsInvalid()
    {
        var days = new List<MessagingHoursDayRequest>
        {
            new(0, new TimeOnly(18, 0), new TimeOnly(8, 0), false), // Open after close
        };
        var cmd = new UpdateMessagingHoursCommand(days);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose() => _context.Dispose();
}
