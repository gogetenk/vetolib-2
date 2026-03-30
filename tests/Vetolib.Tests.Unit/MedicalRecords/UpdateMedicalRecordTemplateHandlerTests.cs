using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.UpdateMedicalRecordTemplate;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class UpdateMedicalRecordTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly UpdateMedicalRecordTemplateHandler _handler;

    public UpdateMedicalRecordTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new UpdateMedicalRecordTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_CustomTemplate_UpdatesSuccessfully()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "Old Name", TemplateCategory.General,
            "old diag", "old treat", "old notes",
            species: null, isSystemTemplate: false, sortOrder: 1).Value;
        _context.MedicalRecordTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new UpdateMedicalRecordTemplateCommand(
            template.Id, "New Name", TemplateCategory.Surgery,
            "new diag", "new treat", "new notes",
            Species.Cat, 5);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Category.Should().Be(TemplateCategory.Surgery);
    }

    [Fact]
    public async Task Handle_SystemTemplate_ReturnsError()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "System", TemplateCategory.Checkup,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: true, sortOrder: 1).Value;
        _context.MedicalRecordTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new UpdateMedicalRecordTemplateCommand(
            template.Id, "Modified", TemplateCategory.Surgery,
            "diag", "treat", "notes", null, 2);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SYSTEM_TEMPLATE_READONLY"));
    }

    [Fact]
    public async Task Handle_NonExistentTemplate_ReturnsNotFound()
    {
        var cmd = new UpdateMedicalRecordTemplateCommand(
            Guid.NewGuid(), "Name", TemplateCategory.General,
            "diag", "treat", "notes", null, 0);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
