using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Queries.GetOwnerConversationById;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class GetOwnerConversationByIdHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherOwnerId = new("33333333-3333-3333-3333-333333333333");

    private readonly MessagingDbContext _context;
    private readonly GetOwnerConversationByIdHandler _handler;

    public GetOwnerConversationByIdHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);
        _handler = new GetOwnerConversationByIdHandler(_context);
    }

    private Conversation SeedConversation(Guid? ownerId = null)
    {
        var conversationResult = Conversation.Create(
            ClinicId,
            ownerId ?? OwnerId,
            patientId: null,
            subject: "Vaccination query",
            category: MessageCategory.Administrative);

        conversationResult.IsSuccess.Should().BeTrue();
        var conversation = conversationResult.Value;

        _context.Conversations.Add(conversation);
        _context.SaveChanges();
        return conversation;
    }

    [Fact]
    public async Task Handle_WhenConversationExistsForOwner_ReturnsSuccess()
    {
        var conversation = SeedConversation();

        var query = new GetOwnerConversationByIdQuery(conversation.Id, OwnerId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(conversation.Id);
        result.Value.Subject.Should().Be("Vaccination query");
        result.Value.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task Handle_WhenConversationDoesNotExist_ReturnsNotFound()
    {
        var query = new GetOwnerConversationByIdQuery(Guid.NewGuid(), OwnerId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToAnotherOwner_ReturnsNotFound()
    {
        // Conversation seeded for OtherOwnerId
        var conversation = SeedConversation(ownerId: OtherOwnerId);

        // Query with OwnerId — should not find it (ownership check)
        var query = new GetOwnerConversationByIdQuery(conversation.Id, OwnerId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenConversationHasInternalNote_NoteIsNotReturned()
    {
        // Build the conversation with messages before the first SaveChanges
        var conversationResult = Conversation.Create(
            ClinicId, OwnerId, patientId: null,
            subject: "Internal note test", category: MessageCategory.Administrative);
        conversationResult.IsSuccess.Should().BeTrue();
        var conversation = conversationResult.Value;

        conversation.AddMessage(MessageSender.Staff, Guid.NewGuid(), "Public reply");
        conversation.AddMessage(MessageSender.Staff, Guid.NewGuid(), "Internal note text", isInternalNote: true);

        _context.Conversations.Add(conversation);
        _context.SaveChanges();

        var query = new GetOwnerConversationByIdQuery(conversation.Id, OwnerId, ClinicId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Messages.Should().NotContain(m => m.IsInternalNote);
        result.Value.Messages.Should().ContainSingle(m => m.Body == "Public reply");
    }

    public void Dispose() => _context.Dispose();
}
