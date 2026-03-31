using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateFollowUpRule;
using Vetolib.Agenda.Application.Commands.DeactivateFollowUpRule;
using Vetolib.Agenda.Application.Commands.UpdateFollowUpRule;
using Vetolib.Agenda.Application.Queries.ListFollowUpRules;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class FollowUpRuleHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly AgendaDbContext _context;
    private readonly CreateFollowUpRuleHandler _createHandler;
    private readonly UpdateFollowUpRuleHandler _updateHandler;
    private readonly DeactivateFollowUpRuleHandler _deactivateHandler;
    private readonly ListFollowUpRulesHandler _listHandler;

    public FollowUpRuleHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _createHandler = new CreateFollowUpRuleHandler(_context);
        _updateHandler = new UpdateFollowUpRuleHandler(_context);
        _deactivateHandler = new DeactivateFollowUpRuleHandler(_context);
        _listHandler = new ListFollowUpRulesHandler(_context);
    }

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateHandler_HappyPath_ReturnsSuccessWithDto()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up");

        var result = await _createHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ConsultationType.Should().Be("Surgery");
        result.Value.FollowUpDays.Should().Be(10);
        result.Value.FollowUpReason.Should().Be("Post-surgery follow-up");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateHandler_DuplicateActiveRule_ReturnsConflict()
    {
        var cmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up");
        await _createHandler.Handle(cmd, CancellationToken.None);

        var duplicateCmd = new CreateFollowUpRuleCommand(ClinicId, "Surgery", 14, "Another reason");

        var result = await _createHandler.Handle(duplicateCmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    // ── Update ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateHandler_ExistingRule_ReturnsUpdatedDto()
    {
        var createResult = await _createHandler.Handle(
            new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up"),
            CancellationToken.None);

        var updateCmd = new UpdateFollowUpRuleCommand(
            createResult.Value.Id, "Vaccination", 21, "Vaccination booster check");

        var updateResult = await _updateHandler.Handle(updateCmd, CancellationToken.None);

        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.ConsultationType.Should().Be("Vaccination");
        updateResult.Value.FollowUpDays.Should().Be(21);
        updateResult.Value.FollowUpReason.Should().Be("Vaccination booster check");
    }

    [Fact]
    public async Task UpdateHandler_NonExistentId_ReturnsNotFound()
    {
        var cmd = new UpdateFollowUpRuleCommand(Guid.NewGuid(), "Surgery", 10, "Post-surgery follow-up");

        var result = await _updateHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── Deactivate ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeactivateHandler_ExistingRule_ReturnsSuccess()
    {
        var createResult = await _createHandler.Handle(
            new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up"),
            CancellationToken.None);

        var result = await _deactivateHandler.Handle(
            new DeactivateFollowUpRuleCommand(createResult.Value.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateHandler_NonExistentId_ReturnsNotFound()
    {
        var result = await _deactivateHandler.Handle(
            new DeactivateFollowUpRuleCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task DeactivateHandler_AlreadyInactive_ReturnsError()
    {
        var createResult = await _createHandler.Handle(
            new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up"),
            CancellationToken.None);

        await _deactivateHandler.Handle(
            new DeactivateFollowUpRuleCommand(createResult.Value.Id),
            CancellationToken.None);

        var result = await _deactivateHandler.Handle(
            new DeactivateFollowUpRuleCommand(createResult.Value.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    // ── List ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListHandler_ReturnsOnlyActiveRules()
    {
        await _createHandler.Handle(
            new CreateFollowUpRuleCommand(ClinicId, "Surgery", 10, "Post-surgery follow-up"),
            CancellationToken.None);

        var toDeactivate = await _createHandler.Handle(
            new CreateFollowUpRuleCommand(ClinicId, "Vaccination", 21, "Vaccination booster check"),
            CancellationToken.None);

        await _deactivateHandler.Handle(
            new DeactivateFollowUpRuleCommand(toDeactivate.Value.Id),
            CancellationToken.None);

        var listResult = await _listHandler.Handle(new ListFollowUpRulesQuery(), CancellationToken.None);

        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().HaveCount(1);
        listResult.Value.First().ConsultationType.Should().Be("Surgery");
    }

    [Fact]
    public async Task ListHandler_EmptyDb_ReturnsEmptyList()
    {
        var listResult = await _listHandler.Handle(new ListFollowUpRulesQuery(), CancellationToken.None);

        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
