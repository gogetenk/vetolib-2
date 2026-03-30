using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.DeleteMedicalRecordTemplate;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class DeleteMedicalRecordTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly DeleteMedicalRecordTemplateHandler _handler;

    public DeleteMedicalRecordTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new DeleteMedicalRecordTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_CustomTemplate_DeletesSuccessfully()
    {
        var template = MedicalRecordTemplate.Create(
            ClinicId, "Custom", TemplateCategory.General,
            "diag", "treat", "notes",
            species: null, isSystemTemplate: false, sortOrder: 0).Value;
        _context.MedicalRecordTemplates.Add(template);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new DeleteMedicalRecordTemplateCommand(template.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var count = await _context.MedicalRecordTemplates.CountAsync();
        count.Should().Be(0);
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

        var result = await _handler.Handle(
            new DeleteMedicalRecordTemplateCommand(template.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SYSTEM_TEMPLATE_READONLY"));
    }

    [Fact]
    public async Task Handle_NonExistentTemplate_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new DeleteMedicalRecordTemplateCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
