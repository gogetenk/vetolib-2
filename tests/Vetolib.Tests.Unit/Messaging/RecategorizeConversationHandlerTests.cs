using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.RecategorizeConversation;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class RecategorizeConversationHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly RecategorizeConversationHandler _sut;

    public RecategorizeConversationHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new RecategorizeConversationHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenConversationExists_UpdatesCategory()
    {
        var conversation = CreateConversation(MessageCategory.Administrative);
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new RecategorizeConversationCommand(conversation.Id, MessageCategory.MedicalUrgency);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(MessageCategory.MedicalUrgency);
    }

    [Fact]
    public async Task Handle_ResetsTriageUncertaintyFlag()
    {
        var conversation = CreateConversation(MessageCategory.Administrative);
        conversation.SetTriageResult(0.5m, isUncertain: true);
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new RecategorizeConversationCommand(conversation.Id, MessageCategory.MedicalQuestion);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsTriageUncertain.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new RecategorizeConversationCommand(Guid.NewGuid(), MessageCategory.MedicalUrgency);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_PersistsNewCategoryToDatabase()
    {
        var conversation = CreateConversation(MessageCategory.Administrative);
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new RecategorizeConversationCommand(conversation.Id, MessageCategory.Feedback);

        await _sut.Handle(cmd, CancellationToken.None);

        var saved = await _context.Conversations.FirstAsync();
        saved.Category.Should().Be(MessageCategory.Feedback);
    }

    private static Conversation CreateConversation(MessageCategory category)
    {
        return Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            null,
            "Test conversation",
            category).Value;
    }

    public void Dispose() => _context.Dispose();
}
