using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.DeleteStaffSchedule;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class DeleteStaffScheduleHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private readonly AgendaDbContext _context;
    private readonly DeleteStaffScheduleHandler _handler;

    public DeleteStaffScheduleHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new DeleteStaffScheduleHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenExists_DeletesAndReturnsSuccess()
    {
        var schedule = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            new TimeOnly(8, 0), new TimeOnly(14, 0), ShiftType.Morning);
        _context.StaffSchedules.Add(schedule.Value);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new DeleteStaffScheduleCommand(schedule.Value.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var remaining = await _context.StaffSchedules.CountAsync();
        remaining.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNotFound()
    {
        var result = await _handler.Handle(new DeleteStaffScheduleCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
