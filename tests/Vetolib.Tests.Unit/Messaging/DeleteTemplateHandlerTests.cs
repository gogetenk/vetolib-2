using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.DeleteTemplate;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class DeleteTemplateHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly DeleteTemplateHandler _sut;

    public DeleteTemplateHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new DeleteTemplateHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenTemplateExists_DeletesAndReturnsSuccess()
    {
        var template = ResponseTemplate.Create(ClinicId, "To Delete", "EN", "AR", null).Value;
        _context.ResponseTemplates.Add(template);
        await _context.SaveChangesAsync();

        var cmd = new DeleteTemplateCommand(template.Id);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var count = await _context.ResponseTemplates.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenTemplateNotFound_ReturnsNotFound()
    {
        var cmd = new DeleteTemplateCommand(Guid.NewGuid());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    public void Dispose() => _context.Dispose();
}
