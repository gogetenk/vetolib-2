using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Commands.ClassificationFeedback;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class ClassificationFeedbackHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly MessagingDbContext _context;
    private readonly ClassificationFeedbackHandler _sut;

    public ClassificationFeedbackHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new ClassificationFeedbackHandler(_context);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WhenMessageClassified_RecordsFeedback(bool isCorrect)
    {
        // Arrange
        var conversation = CreateConversation();
        var message = CreateClassifiedMessage(conversation.Id);
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new ClassificationFeedbackCommand(conversation.Id, message.Id, isCorrect);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Messages.FindAsync(message.Id);
        updated!.ClassificationFeedbackCorrect.Should().Be(isCorrect);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new ClassificationFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), true);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenMessageNotFound_ReturnsNotFound()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ClassificationFeedbackCommand(conversation.Id, Guid.NewGuid(), true);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenMessageNotClassified_ReturnsError()
    {
        var conversation = CreateConversation();
        var messageResult = Message.Create(conversation.Id, MessageSender.Owner, null, "not classified");
        var message = messageResult.Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new ClassificationFeedbackCommand(conversation.Id, message.Id, true);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("NOT_CLASSIFIED"));
    }

    private static Conversation CreateConversation()
    {
        return Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            null,
            "Test conversation",
            MessageCategory.Administrative).Value;
    }

    private static Message CreateClassifiedMessage(Guid conversationId)
    {
        var messageResult = Message.Create(conversationId, MessageSender.Owner, null, "Test message");
        var message = messageResult.Value;
        message.ApplyClassification(
            ClassifiedUrgency.Normal,
            ClassifiedCategory.AdministrativeRequest,
            0.85,
            false);
        return message;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
