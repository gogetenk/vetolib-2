using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;
using Vetolib.Messaging.Application.Commands.AddInternalNote;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class AddInternalNoteHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly AddInternalNoteHandler _sut;

    public AddInternalNoteHandlerTests()
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
            new Claim(ClaimTypes.Role, "Vet")
        ]));
        httpContextAccessor.HttpContext.Returns(httpContext);

        _sut = new AddInternalNoteHandler(_context, httpContextAccessor);
    }

    [Fact]
    public async Task Handle_WhenConversationExists_AddsInternalNote()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new AddInternalNoteCommand(conversation.Id, "This patient needs follow-up blood work");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsInternalNote.Should().BeTrue();
        result.Value.Sender.Should().Be(MessageSender.Vet);
        result.Value.Body.Should().Be("This patient needs follow-up blood work");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new AddInternalNoteCommand(Guid.NewGuid(), "Note body");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenConversationClosed_ReturnsError()
    {
        var conversation = CreateConversation();
        conversation.Close();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new AddInternalNoteCommand(conversation.Id, "Note on closed conversation");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithStaffRole_SetsSenderAsStaff()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserId.ToString()),
            new Claim(ClaimTypes.Role, "Receptionist")
        ]));
        httpContextAccessor.HttpContext.Returns(httpContext);

        var sut = new AddInternalNoteHandler(context, httpContextAccessor);

        var conversation = CreateConversation();
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var cmd = new AddInternalNoteCommand(conversation.Id, "Staff note");

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sender.Should().Be(MessageSender.Staff);

        context.Dispose();
    }

    [Fact]
    public async Task Handle_PersistsMessageToDatabase()
    {
        var conversation = CreateConversation();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new AddInternalNoteCommand(conversation.Id, "Persisted note");

        await _sut.Handle(cmd, CancellationToken.None);

        var messages = await _context.Messages.ToListAsync();
        messages.Should().HaveCount(1);
        messages[0].IsInternalNote.Should().BeTrue();
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

    public void Dispose() => _context.Dispose();
}
