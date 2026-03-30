using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.UpdateTemplate;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class UpdateTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly UpdateTemplateHandler _sut;

    public UpdateTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new UpdateTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenTemplateExists_UpdatesAndReturnsSuccess()
    {
        var template = ResponseTemplate.Create(ClinicId, "Old Name", "Old EN", "عربي قديم", MessageCategory.Administrative).Value;
        _context.ResponseTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new UpdateTemplateCommand(template.Id, "New Name", "New EN", "عربي جديد", MessageCategory.MedicalQuestion);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.ContentEn.Should().Be("New EN");
        result.Value.ContentAr.Should().Be("عربي جديد");
        result.Value.Category.Should().Be(MessageCategory.MedicalQuestion);
    }

    [Fact]
    public async Task Handle_WhenTemplateNotFound_ReturnsNotFound()
    {
        var cmd = new UpdateTemplateCommand(Guid.NewGuid(), "Name", "EN", "AR", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsInvalid()
    {
        var template = ResponseTemplate.Create(ClinicId, "Name", "EN", "AR", null).Value;
        _context.ResponseTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new UpdateTemplateCommand(template.Id, "", "EN", "AR", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_PersistsChangesToDatabase()
    {
        var template = ResponseTemplate.Create(ClinicId, "Original", "EN", "AR", null).Value;
        _context.ResponseTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new UpdateTemplateCommand(template.Id, "Updated", "Updated EN", "Updated AR", MessageCategory.Feedback);

        await _sut.Handle(cmd, CancellationToken.None);

        var saved = await _context.ResponseTemplates.FirstAsync();
        saved.Name.Should().Be("Updated");
        saved.Category.Should().Be(MessageCategory.Feedback);
    }

    public void Dispose() => _context.Dispose();
}
