using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.GetVisitFeedbackStats;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class GetVisitFeedbackStatsHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AppointmentId1 = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid AppointmentId2 = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid AppointmentId3 = new("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private readonly AgendaDbContext _context;
    private readonly GetVisitFeedbackStatsHandler _handler;

    public GetVisitFeedbackStatsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _handler = new GetVisitFeedbackStatsHandler(_context);
    }

    private async Task SeedFeedback(Guid appointmentId, int rating)
    {
        var feedback = VisitFeedback.Create(ClinicId, appointmentId, rating, null, false).Value;
        _context.VisitFeedbacks.Add(feedback);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenNoFeedback_ReturnsZeroStats()
    {
        var result = await _handler.Handle(new GetVisitFeedbackStatsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0);
        result.Value.AverageRating.Should().Be(0);
        result.Value.NpsScore.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMixedRatings_ReturnsCorrectStats()
    {
        await SeedFeedback(AppointmentId1, 5); // promoter
        await SeedFeedback(AppointmentId2, 1); // detractor
        await SeedFeedback(AppointmentId3, 3); // neutral

        var result = await _handler.Handle(new GetVisitFeedbackStatsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
        result.Value.AverageRating.Should().Be(3);
        result.Value.CountStar1.Should().Be(1);
        result.Value.CountStar3.Should().Be(1);
        result.Value.CountStar5.Should().Be(1);
        result.Value.CountStar2.Should().Be(0);
        result.Value.CountStar4.Should().Be(0);
        // NPS: (1 promoter - 1 detractor) / 3 * 100 = 0
        result.Value.NpsScore.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAllPromoters_ReturnsNps100()
    {
        await SeedFeedback(AppointmentId1, 5);
        await SeedFeedback(AppointmentId2, 4);

        var result = await _handler.Handle(new GetVisitFeedbackStatsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NpsScore.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithAllDetractors_ReturnsNpsMinus100()
    {
        await SeedFeedback(AppointmentId1, 1);
        await SeedFeedback(AppointmentId2, 2);

        var result = await _handler.Handle(new GetVisitFeedbackStatsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.NpsScore.Should().Be(-100);
    }

    public void Dispose() => _context.Dispose();
}
