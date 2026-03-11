using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Security.Claims;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Application.Queries.GetConversationById;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class GetConversationByIdHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = new("22222222-2222-2222-2222-222222222222");

    private readonly MessagingDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPatientReader _patientReader;
    private readonly GetConversationByIdHandler _handler;

    public GetConversationByIdHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, publisher);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _patientReader = Substitute.For<IPatientReader>();

        _handler = new GetConversationByIdHandler(
            _context,
            _httpContextAccessor,
            _patientReader,
            NullLogger<GetConversationByIdHandler>.Instance);
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
        MessageCategory category = MessageCategory.Administrative,
        bool isSpam = false)
    {
        var conversationResult = Conversation.Create(
            ClinicId,
            OwnerId,
            patientId: null,
            subject: "Test subject",
            category: category);

        conversationResult.IsSuccess.Should().BeTrue();
        var conversation = conversationResult.Value;

        _context.Conversations.Add(conversation);
        _context.SaveChanges();
        return conversation;
    }

    [Fact]
    public async Task Handle_WhenConversationExists_ReturnsSuccess()
    {
        SetRole("Admin");
        var conversation = SeedConversation();

        var query = new GetConversationByIdQuery(conversation.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(conversation.Id);
        result.Value.Subject.Should().Be("Test subject");
    }

    [Fact]
    public async Task Handle_WhenConversationDoesNotExist_ReturnsNotFound()
    {
        SetRole("Admin");

        var query = new GetConversationByIdQuery(Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenReceptionistAccessesMedicalCategory_ReturnsForbidden()
    {
        SetRole("Receptionist");
        var conversation = SeedConversation(category: MessageCategory.MedicalUrgency);

        var query = new GetConversationByIdQuery(conversation.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenAssistantAccessesMedicalCategory_ReturnsForbidden()
    {
        SetRole("Assistant");
        var conversation = SeedConversation(category: MessageCategory.PostOperativeFollowUp);

        var query = new GetConversationByIdQuery(conversation.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenVetAccessesMedicalCategory_ReturnsSuccess()
    {
        SetRole("Vet");
        var conversation = SeedConversation(category: MessageCategory.MedicalQuestion);

        var query = new GetConversationByIdQuery(conversation.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(conversation.Id);
    }

    [Fact]
    public async Task Handle_WhenReceptionistAccessesNonMedicalCategory_ReturnsSuccess()
    {
        SetRole("Receptionist");
        var conversation = SeedConversation(category: MessageCategory.Administrative);

        var query = new GetConversationByIdQuery(conversation.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    public void Dispose() => _context.Dispose();
}
