using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Queries.ListConversations;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class ListConversationsHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ListConversationsHandler _handler;

    public ListConversationsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _handler = new ListConversationsHandler(_context, _httpContextAccessor);
    }

    private void SetRole(string role)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(user);
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }

    private Conversation SeedConversation(
        MessageCategory category,
        string? assignedToRole = null,
        bool isSpam = false)
    {
        var conversationResult = Conversation.Create(
            ClinicId,
            OwnerId,
            patientId: null,
            subject: $"Subject for {category}",
            category: category);

        conversationResult.IsSuccess.Should().BeTrue();
        var conversation = conversationResult.Value;

        if (assignedToRole is not null)
            conversation.AssignTo(null, assignedToRole);

        if (isSpam)
            conversation.MarkAsSpam();

        _context.Conversations.Add(conversation);
        _context.SaveChanges();
        return conversation;
    }

    private ListConversationsQuery DefaultQuery(int page = 1, int pageSize = 20) =>
        new(Status: null, Category: null, FromDate: null, ToDate: null, Page: page, PageSize: pageSize);

    [Fact]
    public async Task Handle_WhenAdminRole_ReturnsAllNonSpamConversations()
    {
        SetRole("Admin");
        SeedConversation(MessageCategory.Administrative);
        SeedConversation(MessageCategory.MedicalUrgency);
        SeedConversation(MessageCategory.AppointmentRequest, isSpam: true);

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenReceptionistRole_ReturnsOnlyNonMedicalConversations()
    {
        SetRole("Receptionist");
        SeedConversation(MessageCategory.Administrative);
        SeedConversation(MessageCategory.AppointmentRequest);
        SeedConversation(MessageCategory.MedicalUrgency);   // Should be excluded
        SeedConversation(MessageCategory.MedicalQuestion);  // Should be excluded

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().NotContain(c =>
            c.Category == MessageCategory.MedicalUrgency ||
            c.Category == MessageCategory.MedicalQuestion);
    }

    [Fact]
    public async Task Handle_WhenVetRole_ReturnsMedicalConversations()
    {
        SetRole("Vet");
        SeedConversation(MessageCategory.MedicalUrgency);
        SeedConversation(MessageCategory.PostOperativeFollowUp);
        SeedConversation(MessageCategory.Administrative);  // Should be excluded (non-medical, not assigned to Vet)

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().OnlyContain(c =>
            c.Category == MessageCategory.MedicalUrgency ||
            c.Category == MessageCategory.PostOperativeFollowUp);
    }

    [Fact]
    public async Task Handle_WhenUnknownRole_ReturnsEmptyList()
    {
        SetRole("Unknown");
        SeedConversation(MessageCategory.Administrative);

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoConversations_ReturnsEmptyList()
    {
        SetRole("Admin");

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteredByStatus_ReturnsOnlyMatchingConversations()
    {
        SetRole("Admin");
        SeedConversation(MessageCategory.Administrative);  // Status = Open by default

        var query = new ListConversationsQuery(
            Status: ConversationStatus.Open,
            Category: null,
            FromDate: null,
            ToDate: null,
            Page: 1,
            PageSize: 20);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.All(c => c.Status == ConversationStatus.Open).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenFilteredByCategory_ReturnsOnlyMatchingConversations()
    {
        SetRole("Admin");
        SeedConversation(MessageCategory.Administrative);
        SeedConversation(MessageCategory.AppointmentRequest);

        var query = new ListConversationsQuery(
            Status: null,
            Category: MessageCategory.Administrative,
            FromDate: null,
            ToDate: null,
            Page: 1,
            PageSize: 20);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Category.Should().Be(MessageCategory.Administrative);
    }

    [Fact]
    public async Task Handle_SpamConversationsAreAlwaysExcluded()
    {
        SetRole("Admin");
        SeedConversation(MessageCategory.Administrative, isSpam: true);

        var result = await _handler.Handle(DefaultQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
