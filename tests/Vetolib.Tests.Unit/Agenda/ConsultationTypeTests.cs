using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Agenda.Application.Commands.CreateConsultationType;
using Vetolib.Agenda.Application.Commands.DeactivateConsultationType;
using Vetolib.Agenda.Application.Commands.UpdateConsultationType;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Application.Queries.ListConsultationTypes;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class ConsultationTypeTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly AgendaDbContext _context;
    private readonly CreateConsultationTypeHandler _createHandler;
    private readonly UpdateConsultationTypeHandler _updateHandler;
    private readonly DeactivateConsultationTypeHandler _deactivateHandler;
    private readonly ListConsultationTypesHandler _listHandler;

    public ConsultationTypeTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<AgendaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AgendaDbContext(options, clinicContext, publisher);
        _createHandler = new CreateConsultationTypeHandler(_context);
        _updateHandler = new UpdateConsultationTypeHandler(_context);
        _deactivateHandler = new DeactivateConsultationTypeHandler(_context);
        _listHandler = new ListConsultationTypesHandler(_context);
    }

    // ---- Domain: ConsultationType.Create ----

    [Fact]
    public void Create_WithValidValues_ReturnsSuccess()
    {
        var result = ConsultationType.Create(ClinicId, "General Consultation", 30);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("General Consultation");
        result.Value.DurationMinutes.Should().Be(30);
        result.Value.IsActive.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsInvalid()
    {
        var result = ConsultationType.Create(ClinicId, "", 30);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(181)]
    [InlineData(300)]
    public void Create_WithInvalidDuration_ReturnsInvalid(int duration)
    {
        var result = ConsultationType.Create(ClinicId, "Test Type", duration);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "durationMinutes");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(180)]
    public void Create_WithBoundaryDurations_ReturnsSuccess(int duration)
    {
        var result = ConsultationType.Create(ClinicId, "Test Type", duration);

        result.IsSuccess.Should().BeTrue();
        result.Value.DurationMinutes.Should().Be(duration);
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = ConsultationType.Create(Guid.Empty, "General Consultation", 30);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithNameExceeding100Chars_ReturnsInvalid()
    {
        var longName = new string('A', 101);
        var result = ConsultationType.Create(ClinicId, longName, 30);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "name");
    }

    // ---- Domain: Update and Deactivate ----

    [Fact]
    public void Update_WithValidValues_ReturnsSuccess()
    {
        var createResult = ConsultationType.Create(ClinicId, "General Consultation", 30);
        var ct = createResult.Value;

        var updateResult = ct.Update("Updated Consultation", 45, 1, true);

        updateResult.IsSuccess.Should().BeTrue();
        ct.Name.Should().Be("Updated Consultation");
        ct.DurationMinutes.Should().Be(45);
        ct.SortOrder.Should().Be(1);
        ct.RequiresVetSelection.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ActiveType_ReturnsSuccess()
    {
        var ct = ConsultationType.Create(ClinicId, "General Consultation", 30).Value;

        var result = ct.Deactivate();

        result.IsSuccess.Should().BeTrue();
        ct.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ReturnsError()
    {
        var ct = ConsultationType.Create(ClinicId, "General Consultation", 30).Value;
        ct.Deactivate();

        var result = ct.Deactivate();

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

    // ---- Handler: ListConsultationTypes returns only active ----

    [Fact]
    public async Task ListConsultationTypes_ReturnsOnlyActiveTypes()
    {
        // Arrange
        var activeCmd = new CreateConsultationTypeCommand(ClinicId, "Active Type", 30);
        var activeResult = await _createHandler.Handle(activeCmd, CancellationToken.None);
        activeResult.IsSuccess.Should().BeTrue();

        var toDeactivate = new CreateConsultationTypeCommand(ClinicId, "Inactive Type", 20);
        var toDeactivateResult = await _createHandler.Handle(toDeactivate, CancellationToken.None);
        toDeactivateResult.IsSuccess.Should().BeTrue();

        await _deactivateHandler.Handle(
            new DeactivateConsultationTypeCommand(toDeactivateResult.Value.Id),
            CancellationToken.None);

        // Act
        var listResult = await _listHandler.Handle(new ListConsultationTypesQuery(), CancellationToken.None);

        // Assert
        listResult.IsSuccess.Should().BeTrue();
        listResult.Value.Should().HaveCount(1);
        listResult.Value.First().Name.Should().Be("Active Type");
    }

    // ---- Handler: Create happy path ----

    [Fact]
    public async Task CreateHandler_HappyPath_ReturnsSuccessWithDto()
    {
        var cmd = new CreateConsultationTypeCommand(ClinicId, "Vaccination", 20, 1, false);

        var result = await _createHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Vaccination");
        result.Value.DurationMinutes.Should().Be(20);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
    }

    // ---- Handler: Update ----

    [Fact]
    public async Task UpdateHandler_ExistingType_ReturnsUpdatedDto()
    {
        var createResult = await _createHandler.Handle(
            new CreateConsultationTypeCommand(ClinicId, "General Consultation", 30),
            CancellationToken.None);

        var updateCmd = new UpdateConsultationTypeCommand(
            createResult.Value.Id, "Updated Consultation", 45, 2, true);

        var updateResult = await _updateHandler.Handle(updateCmd, CancellationToken.None);

        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Name.Should().Be("Updated Consultation");
        updateResult.Value.DurationMinutes.Should().Be(45);
    }

    [Fact]
    public async Task UpdateHandler_NonExistentId_ReturnsNotFound()
    {
        var cmd = new UpdateConsultationTypeCommand(Guid.NewGuid(), "X", 30, 0, false);

        var result = await _updateHandler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
