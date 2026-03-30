using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.CreateMedicalRecordTemplate;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreateMedicalRecordTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly CreateMedicalRecordTemplateHandler _handler;

    public CreateMedicalRecordTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new CreateMedicalRecordTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesCustomTemplate()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            ClinicId, "My Custom Template", TemplateCategory.General,
            "Custom diagnosis", "Custom treatment", "Custom notes",
            Species: null, SortOrder: 10);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Custom Template");
        result.Value.IsSystemTemplate.Should().BeFalse();
        result.Value.SortOrder.Should().Be(10);
    }

    [Fact]
    public async Task Handle_HappyPath_PersistsToDatabase()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            ClinicId, "Persisted Template", TemplateCategory.Surgery,
            "Surgery diag", "Surgery treat", "Surgery notes",
            Species.Dog, SortOrder: 1);

        await _handler.Handle(cmd, CancellationToken.None);

        var count = await _context.MedicalRecordTemplates.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsInvalid()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            ClinicId, "", TemplateCategory.General,
            "diag", "treat", "notes",
            Species: null, SortOrder: 0);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_AlwaysCreatesNonSystemTemplate()
    {
        var cmd = new CreateMedicalRecordTemplateCommand(
            ClinicId, "Template", TemplateCategory.Dental,
            "diag", "treat", "notes",
            Species: null, SortOrder: 0);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSystemTemplate.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}
