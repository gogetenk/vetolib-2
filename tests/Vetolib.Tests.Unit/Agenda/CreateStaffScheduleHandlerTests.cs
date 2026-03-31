using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateStaffSchedule;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class CreateStaffScheduleHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    private readonly AgendaDbContext _context;
    private readonly CreateStaffScheduleHandler _handler;

    public CreateStaffScheduleHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new CreateStaffScheduleHandler(_context);
    }

    private CreateStaffScheduleCommand BuildCommand(
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        Guid? userId = null) =>
        new(
            ClinicId: ClinicId,
            UserId: userId ?? UserId,
            UserName: "Dr. Fatima Al-Zahra",
            Date: FutureDate,
            StartTime: startTime ?? new TimeOnly(8, 0),
            EndTime: endTime ?? new TimeOnly(14, 0),
            ShiftType: ShiftType.Morning,
            IsAvailable: true);

    [Fact]
    public async Task Handle_WhenValid_ReturnsSuccess()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(UserId);
        result.Value.UserName.Should().Be("Dr. Fatima Al-Zahra");
        result.Value.ShiftType.Should().Be(ShiftType.Morning);
    }

    [Fact]
    public async Task Handle_WhenValid_PersistsToDatabase()
    {
        var cmd = BuildCommand();

        await _handler.Handle(cmd, CancellationToken.None);

        var saved = await _context.StaffSchedules.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
    }

    [Fact]
    public async Task Handle_WhenOverlappingSchedule_ReturnsError()
    {
        // Seed an existing schedule 08:00-14:00
        var existing = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            new TimeOnly(8, 0), new TimeOnly(14, 0), ShiftType.Morning);
        _context.StaffSchedules.Add(existing.Value);
        await _context.SaveChangesAsync();

        // Try to create overlapping 10:00-16:00
        var cmd = BuildCommand(startTime: new TimeOnly(10, 0), endTime: new TimeOnly(16, 0));

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SCHEDULE_OVERLAP"));
    }

    [Fact]
    public async Task Handle_WhenNoOverlap_ReturnsSuccess()
    {
        // Seed an existing schedule 08:00-12:00
        var existing = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            new TimeOnly(8, 0), new TimeOnly(12, 0), ShiftType.Morning);
        _context.StaffSchedules.Add(existing.Value);
        await _context.SaveChangesAsync();

        // Create non-overlapping 14:00-18:00
        var cmd = BuildCommand(startTime: new TimeOnly(14, 0), endTime: new TimeOnly(18, 0));

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenDifferentUser_AllowsOverlap()
    {
        // Seed existing schedule for UserId
        var existing = StaffSchedule.Create(
            ClinicId, UserId, "Dr. Fatima Al-Zahra", FutureDate,
            new TimeOnly(8, 0), new TimeOnly(14, 0), ShiftType.Morning);
        _context.StaffSchedules.Add(existing.Value);
        await _context.SaveChangesAsync();

        // Different user, same time
        var otherUserId = Guid.NewGuid();
        var cmd = BuildCommand(startTime: new TimeOnly(8, 0), endTime: new TimeOnly(14, 0), userId: otherUserId);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenInvalidDomain_ReturnsInvalid()
    {
        // EndTime before StartTime — caught by domain factory
        var cmd = new CreateStaffScheduleCommand(
            ClinicId: ClinicId,
            UserId: UserId,
            UserName: "Dr. Fatima Al-Zahra",
            Date: FutureDate,
            StartTime: new TimeOnly(14, 0),
            EndTime: new TimeOnly(8, 0),
            ShiftType: ShiftType.Morning,
            IsAvailable: true);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
