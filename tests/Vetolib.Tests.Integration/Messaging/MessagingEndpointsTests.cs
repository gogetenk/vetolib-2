using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Messaging.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Messaging;

/// <summary>
/// Integration tests for Messaging endpoints.
/// Validates HTTP contract, auth, and serialization for the critical conversation endpoints.
/// </summary>
public sealed class MessagingEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public MessagingEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── Helper ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an outbound conversation via the Admin endpoint and returns
    /// the resulting ConversationDto so dependent tests can reuse it.
    /// </summary>
    private async Task<ConversationDto> CreateOutboundConversationAsync(HttpClient adminClient)
    {
        var request = new OutboundConversationRequest(
            OwnerId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            Subject: "Vaccination reminder for Simba",
            InitialMessageBody: "Dear owner, your pet is due for vaccination.",
            Category: MessageCategory.Administrative);

        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/conversations/outbound", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            new[] { HttpStatusCode.OK, HttpStatusCode.Created },
            $"CreateOutboundConversation failed with {response.StatusCode}");

        var dto = await response.Content.ReadFromJsonAsync<ConversationDto>(JsonOpts);
        dto.Should().NotBeNull();
        return dto!;
    }

    // ── POST /api/v1/messaging/conversations/outbound ──────────────────────

    [Fact]
    public async Task CreateOutboundConversation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new OutboundConversationRequest(
            OwnerId: Guid.NewGuid(),
            PatientId: Guid.NewGuid(),
            Subject: "Annual checkup reminder for Bella",
            InitialMessageBody: "Dear Mr. Al-Rashid, Bella is due for her annual checkup.",
            Category: MessageCategory.AppointmentRequest);

        // Act
        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/conversations/outbound", request, JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ConversationDto>(JsonOpts);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Subject.Should().Be("Annual checkup reminder for Bella");
        body.Category.Should().Be(MessageCategory.AppointmentRequest);
        body.ClinicId.Should().Be(TestClinicId);
    }

    [Fact]
    public async Task CreateOutboundConversation_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new OutboundConversationRequest(
            Guid.NewGuid(), null, "Test", "Body");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/messaging/conversations/outbound", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOutboundConversation_NonAdminRole_Returns403()
    {
        // Arrange — receptionist should not be able to create outbound conversations
        var receptionistClient = CreateReceptionistClient();
        var request = new OutboundConversationRequest(
            Guid.NewGuid(), null, "Test subject", "Test body");

        // Act
        var response = await receptionistClient.PostAsJsonAsync(
            "/api/v1/messaging/conversations/outbound", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/messaging/conversations ────────────────────────────────

    [Fact]
    public async Task ListConversations_Authenticated_Returns200()
    {
        // Arrange — create a conversation first so the list is non-empty
        var adminClient = CreateAdminClient();
        await CreateOutboundConversationAsync(adminClient);

        // Act
        var response = await adminClient.GetAsync("/api/v1/messaging/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var conversations = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<ConversationDto>>(JsonOpts);
        conversations.Should().NotBeNull();
        conversations.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ListConversations_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/messaging/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListConversations_FilterByStatus_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        await CreateOutboundConversationAsync(adminClient);

        // Act — filter by Open status
        var response = await adminClient.GetAsync(
            "/api/v1/messaging/conversations?status=Open");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var conversations = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<ConversationDto>>(JsonOpts);
        conversations.Should().NotBeNull();
    }

    // ── GET /api/v1/messaging/conversations/{id} ───────────────────────────

    [Fact]
    public async Task GetConversationById_ExistingId_Returns200WithMessages()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content
            .ReadFromJsonAsync<ConversationWithMessagesDto>(JsonOpts);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(created.Id);
        dto.Subject.Should().Be(created.Subject);
        dto.Messages.Should().NotBeNull();
        // Outbound conversation should have at least the initial message
        dto.Messages.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetConversationById_NonExistentId_Returns404()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConversationById_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PATCH /api/v1/messaging/conversations/{id}/status ──────────────────

    [Fact]
    public async Task ChangeConversationStatus_Resolve_Returns200()
    {
        // Arrange — create a conversation to resolve
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        var statusRequest = new ConversationStatusChangeRequest(ConversationStatusAction.Resolve);

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/status",
            statusRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeConversationStatus_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var statusRequest = new ConversationStatusChangeRequest(ConversationStatusAction.Resolve);

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/status",
            statusRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeConversationStatus_Unauthenticated_Returns401()
    {
        // Arrange
        var statusRequest = new ConversationStatusChangeRequest(ConversationStatusAction.Resolve);

        // Act
        var response = await Client.WithoutAuth().PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/status",
            statusRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/messaging/conversations/{id}/reply ────────────────────

    [Fact]
    public async Task SendReply_ValidRequest_ReturnsSuccess()
    {
        // Arrange — create a conversation first
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        var replyRequest = new StaffReplyRequest(
            Body: "Thank you for your message. We have scheduled the appointment.");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/reply",
            replyRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendReply_NonExistentConversation_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var replyRequest = new StaffReplyRequest(Body: "Test reply");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/reply",
            replyRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendReply_Unauthenticated_Returns401()
    {
        // Arrange
        var replyRequest = new StaffReplyRequest(Body: "Test reply");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/reply",
            replyRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/messaging/conversations/{id}/notes ────────────────────

    [Fact]
    public async Task AddInternalNote_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        var noteRequest = new InternalNoteRequest(Body: "Owner seems anxious about the surgery.");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/notes",
            noteRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddInternalNote_Unauthenticated_Returns401()
    {
        // Arrange
        var noteRequest = new InternalNoteRequest(Body: "Test note");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/notes",
            noteRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
