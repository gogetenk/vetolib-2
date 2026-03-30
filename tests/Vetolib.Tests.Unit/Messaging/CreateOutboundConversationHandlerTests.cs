using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;
using Vetolib.Messaging.Application.Commands.CreateOutboundConversation;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class CreateOutboundConversationHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly MessagingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CreateOutboundConversationHandler _sut;

    public CreateOutboundConversationHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        ]));
        httpContextAccessor.HttpContext.Returns(httpContext);

        _publishEndpoint = Substitute.For<IPublishEndpoint>();

        _sut = new CreateOutboundConversationHandler(_context, clinicContext, httpContextAccessor, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesConversationAndMessage()
    {
        var cmd = new CreateOutboundConversationCommand(
            OwnerId, null, "Follow-up", "Hello, your pet's results are ready.", MessageCategory.Administrative);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OwnerId.Should().Be(OwnerId);
        result.Value.Subject.Should().Be("Follow-up");
        result.Value.Category.Should().Be(MessageCategory.Administrative);

        var saved = await _context.Conversations.FirstAsync();
        saved.ClinicId.Should().Be(ClinicId);
    }

    [Fact]
    public async Task Handle_WithPatientId_SetsPatientIdOnConversation()
    {
        var patientId = Guid.NewGuid();
        var cmd = new CreateOutboundConversationCommand(
            OwnerId, patientId, "Vaccination reminder", "Time for annual vaccines", MessageCategory.Administrative);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(patientId);
    }

    [Fact]
    public async Task Handle_PublishesOutboundConversationCreatedEvent()
    {
        var cmd = new CreateOutboundConversationCommand(
            OwnerId, null, "Test", "Test body message", MessageCategory.Administrative);

        await _sut.Handle(cmd, CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Any<Vetolib.Messaging.Contracts.Events.OutboundConversationCreatedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithLongMessageBody_TruncatesPreviewInEvent()
    {
        var longBody = new string('A', 200);
        var cmd = new CreateOutboundConversationCommand(
            OwnerId, null, "Test", longBody, MessageCategory.Administrative);

        await _sut.Handle(cmd, CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<Vetolib.Messaging.Contracts.Events.OutboundConversationCreatedEvent>(
                e => e.MessagePreview.Length <= 104), // 100 chars + "..."
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyOwnerId_ReturnsInvalid()
    {
        var cmd = new CreateOutboundConversationCommand(
            Guid.Empty, null, "Test", "Body", MessageCategory.Administrative);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithEmptySubject_ReturnsInvalid()
    {
        var cmd = new CreateOutboundConversationCommand(
            OwnerId, null, "", "Body text", MessageCategory.Administrative);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    public void Dispose() => _context.Dispose();
}
