using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.CreateTemplate;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class CreateTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly CreateTemplateHandler _sut;

    public CreateTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new CreateTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesTemplateAndReturnsSuccess()
    {
        var cmd = new CreateTemplateCommand(
            ClinicId, "Greeting", "Hello, how can we help?", "مرحبا، كيف يمكننا مساعدتك؟", MessageCategory.Administrative);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Greeting");
        result.Value.ContentEn.Should().Be("Hello, how can we help?");
        result.Value.ContentAr.Should().Be("مرحبا، كيف يمكننا مساعدتك؟");
        result.Value.Category.Should().Be(MessageCategory.Administrative);
    }

    [Fact]
    public async Task Handle_PersistsTemplateToDatabase()
    {
        var cmd = new CreateTemplateCommand(
            ClinicId, "Test", "English content", "محتوى عربي", null);

        await _sut.Handle(cmd, CancellationToken.None);

        var templates = await _context.ResponseTemplates.ToListAsync();
        templates.Should().HaveCount(1);
        templates[0].ClinicId.Should().Be(ClinicId);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsInvalid()
    {
        var cmd = new CreateTemplateCommand(ClinicId, "", "Content", "محتوى", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithEmptyContentEn_ReturnsInvalid()
    {
        var cmd = new CreateTemplateCommand(ClinicId, "Name", "", "محتوى", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithEmptyContentAr_ReturnsInvalid()
    {
        var cmd = new CreateTemplateCommand(ClinicId, "Name", "Content", "", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithNullCategory_CreatesTemplateWithoutCategory()
    {
        var cmd = new CreateTemplateCommand(ClinicId, "Generic", "English", "عربي", null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
