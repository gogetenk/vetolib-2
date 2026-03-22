using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Queries.GetClassificationAccuracy;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class GetClassificationAccuracyHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VetUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly GetClassificationAccuracyHandler _sut;

    public GetClassificationAccuracyHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _sut = new GetClassificationAccuracyHandler(_context);
    }

    [Fact]
    public async Task Handle_WhenNoClassifiedMessages_ReturnsZeroAccuracy()
    {
        var result = await _sut.Handle(new GetClassificationAccuracyQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalClassified.Should().Be(0);
        result.Value.AccuracyRate.Should().Be(0);
        result.Value.TotalCorrected.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithFeedback_CalculatesAccuracyRate()
    {
        // Arrange: 3 classified messages, 2 confirmed correct, 1 incorrect
        var conversationId = CreateAndSaveConversation();

        var m1 = CreateClassifiedMessage(conversationId);
        m1.RecordClassificationFeedback(true);
        _context.Messages.Add(m1);

        var m2 = CreateClassifiedMessage(conversationId);
        m2.RecordClassificationFeedback(true);
        _context.Messages.Add(m2);

        var m3 = CreateClassifiedMessage(conversationId);
        m3.RecordClassificationFeedback(false);
        _context.Messages.Add(m3);

        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new GetClassificationAccuracyQuery(), CancellationToken.None);

        // Assert: 2 correct out of 3 evaluated = 0.6667
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalClassified.Should().Be(3);
        result.Value.AccuracyRate.Should().BeApproximately(0.6667, 0.001);
    }

    [Fact]
    public async Task Handle_WithOverrides_CountsAsCorrections()
    {
        // Arrange: 1 overridden message (original category differs from new)
        var conversationId = CreateAndSaveConversation();

        var m1 = CreateClassifiedMessage(conversationId);
        m1.OverrideClassification(VetUserId, ClassifiedUrgency.Critical, ClassifiedCategory.MedicalConcern);
        _context.Messages.Add(m1);

        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(new GetClassificationAccuracyQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalClassified.Should().Be(1);
        result.Value.TotalCorrected.Should().Be(1);
        result.Value.MostCommonCorrections.Should().HaveCount(1);
        result.Value.MostCommonCorrections[0].FromCategory.Should().Be(ClassifiedCategory.AdministrativeRequest);
        result.Value.MostCommonCorrections[0].ToCategory.Should().Be(ClassifiedCategory.MedicalConcern);
    }

    [Fact]
    public async Task Handle_WithMixedFeedbackAndOverrides_CalculatesCorrectly()
    {
        var conversationId = CreateAndSaveConversation();

        // 2 confirmed correct
        for (var i = 0; i < 2; i++)
        {
            var m = CreateClassifiedMessage(conversationId);
            m.RecordClassificationFeedback(true);
            _context.Messages.Add(m);
        }

        // 1 overridden (corrected)
        var overridden = CreateClassifiedMessage(conversationId);
        overridden.OverrideClassification(VetUserId, ClassifiedUrgency.High, ClassifiedCategory.MedicalConcern);
        _context.Messages.Add(overridden);

        // 1 classified but no feedback yet
        var noFeedback = CreateClassifiedMessage(conversationId);
        _context.Messages.Add(noFeedback);

        await _context.SaveChangesAsync();

        var result = await _sut.Handle(new GetClassificationAccuracyQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalClassified.Should().Be(4);
        result.Value.TotalCorrected.Should().Be(1);
        // 2 correct / (2 correct + 1 corrected) = 0.6667
        result.Value.AccuracyRate.Should().BeApproximately(0.6667, 0.001);
    }

    private Guid CreateAndSaveConversation()
    {
        var conversation = Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            null,
            "Test conversation",
            MessageCategory.Administrative).Value;
        _context.Conversations.Add(conversation);
        _context.SaveChanges();
        return conversation.Id;
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
