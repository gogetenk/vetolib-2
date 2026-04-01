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

    // ── PATCH /api/v1/messaging/conversations/{id}/transfer ────────────────

    [Fact]
    public async Task TransferConversation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);
        var transferRequest = new ConversationTransferRequest(
            AssignedToUserId: Guid.NewGuid(),
            AssignedToRole: "Vet");

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/transfer",
            transferRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TransferConversation_Unauthenticated_Returns401()
    {
        // Arrange
        var transferRequest = new ConversationTransferRequest(Guid.NewGuid(), "Vet");

        // Act
        var response = await Client.WithoutAuth().PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/transfer",
            transferRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PATCH /api/v1/messaging/conversations/{id}/category ────────────────

    [Fact]
    public async Task RecategorizeConversation_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);
        var recatRequest = new ConversationRecategorizeRequest(MessageCategory.MedicalUrgency);

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/category",
            recatRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RecategorizeConversation_Unauthenticated_Returns401()
    {
        // Arrange
        var recatRequest = new ConversationRecategorizeRequest(MessageCategory.Other);

        // Act
        var response = await Client.WithoutAuth().PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/category",
            recatRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/messaging/conversations/{id}/spam ─────────────────────

    [Fact]
    public async Task MarkAsSpam_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        // Act
        var response = await adminClient.PostAsync(
            $"/api/v1/messaging/conversations/{created.Id}/spam", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MarkAsSpam_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/spam", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/messaging/conversations/{id}/convert-to-appointment ───

    [Fact]
    public async Task ConvertToAppointment_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);
        var request = new ConvertToAppointmentRequest(
            PreferredDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Notes: "Owner prefers morning appointments");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/convert-to-appointment",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task ConvertToAppointment_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new ConvertToAppointmentRequest(null, null);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/convert-to-appointment",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /conversations/{conversationId}/messages/{messageId}/add-to-record ──

    [Fact]
    public async Task AddMessageToRecord_ValidRequest_ReturnsSuccessOrNotFound()
    {
        // Arrange — create a conversation with a message
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        // Get the conversation to retrieve a real message ID
        var getResponse = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{created.Id}");
        var conversation = await getResponse.Content
            .ReadFromJsonAsync<ConversationWithMessagesDto>(JsonOpts);
        var messageId = conversation!.Messages.First().Id;

        // Act
        var response = await adminClient.PostAsync(
            $"/api/v1/messaging/conversations/{created.Id}/messages/{messageId}/add-to-record",
            null);

        // Assert — may return OK or NotFound (if patient has no medical record yet)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddMessageToRecord_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/messages/{Guid.NewGuid()}/add-to-record",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddMessageToRecord_ReceptionistRole_Returns403()
    {
        // Arrange — receptionist should not be able to add to medical records
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.PostAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/messages/{Guid.NewGuid()}/add-to-record",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/messaging/conversations/{id}/summary ───────────────────

    [Fact]
    public async Task GetConversationSummary_ExistingConversation_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{created.Id}/summary");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetConversationSummary_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PATCH /conversations/{id}/messages/{messageId}/classify ─────────────

    [Fact]
    public async Task OverrideClassification_ValidRequest_ReturnsExpectedStatus()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);
        var getResponse = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{created.Id}");
        var conversation = await getResponse.Content
            .ReadFromJsonAsync<ConversationWithMessagesDto>(JsonOpts);
        var messageId = conversation!.Messages.First().Id;

        var overrideRequest = new ClassifyMessageOverrideRequest(
            Urgency: ClassifiedUrgency.High,
            Category: ClassifiedCategory.MedicalConcern);

        // Act
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/messages/{messageId}/classify",
            overrideRequest,
            JsonOpts);

        // Assert — outbound staff messages are not AI-classified, so override returns 422
        // (NOT_CLASSIFIED). This is correct domain behavior.
        // TI verifies the endpoint is wired and responds (not 500/404).
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task OverrideClassification_Unauthenticated_Returns401()
    {
        // Arrange
        var overrideRequest = new ClassifyMessageOverrideRequest(
            ClassifiedUrgency.Low, ClassifiedCategory.Other);

        // Act
        var response = await Client.WithoutAuth().PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/messages/{Guid.NewGuid()}/classify",
            overrideRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OverrideClassification_ReceptionistRole_Returns403()
    {
        // Arrange — receptionist should not override classification
        var receptionistClient = CreateReceptionistClient();
        var overrideRequest = new ClassifyMessageOverrideRequest(
            ClassifiedUrgency.Low, ClassifiedCategory.Other);

        // Act
        var response = await receptionistClient.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/messages/{Guid.NewGuid()}/classify",
            overrideRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /conversations/{id}/messages/{messageId}/classify/feedback ─────

    [Fact]
    public async Task ClassificationFeedback_ValidRequest_ReturnsExpectedStatus()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var created = await CreateOutboundConversationAsync(adminClient);
        var getResponse = await adminClient.GetAsync(
            $"/api/v1/messaging/conversations/{created.Id}");
        var conversation = await getResponse.Content
            .ReadFromJsonAsync<ConversationWithMessagesDto>(JsonOpts);
        var messageId = conversation!.Messages.First().Id;

        var feedbackRequest = new ClassificationFeedbackRequest(IsCorrect: true);

        // Act
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{created.Id}/messages/{messageId}/classify/feedback",
            feedbackRequest,
            JsonOpts);

        // Assert — outbound staff messages are not AI-classified, so feedback returns 422
        // (NOT_CLASSIFIED). This is correct domain behavior.
        // TI verifies the endpoint is wired and responds (not 500/404).
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ClassificationFeedback_Unauthenticated_Returns401()
    {
        // Arrange
        var feedbackRequest = new ClassificationFeedbackRequest(IsCorrect: false);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{Guid.NewGuid()}/messages/{Guid.NewGuid()}/classify/feedback",
            feedbackRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/messaging/stats/classification-accuracy ────────────────

    [Fact]
    public async Task GetClassificationAccuracy_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync(
            "/api/v1/messaging/stats/classification-accuracy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetClassificationAccuracy_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            "/api/v1/messaging/stats/classification-accuracy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetClassificationAccuracy_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync(
            "/api/v1/messaging/stats/classification-accuracy");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/messaging/settings/hours ───────────────────────────────

    [Fact]
    public async Task GetMessagingHours_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync(
            "/api/v1/messaging/settings/hours");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMessagingHours_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            "/api/v1/messaging/settings/hours");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMessagingHours_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync(
            "/api/v1/messaging/settings/hours");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── PUT /api/v1/messaging/settings/hours ───────────────────────────────

    [Fact]
    public async Task UpdateMessagingHours_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new UpdateMessagingHoursRequest(
            Days: new List<MessagingHoursDayRequest>
            {
                new(0, new TimeOnly(8, 0), new TimeOnly(18, 0), false),
                new(1, new TimeOnly(8, 0), new TimeOnly(18, 0), false),
                new(5, new TimeOnly(0, 0), new TimeOnly(0, 0), true)
            });

        // Act
        var response = await adminClient.PutAsJsonAsync(
            "/api/v1/messaging/settings/hours",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateMessagingHours_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new UpdateMessagingHoursRequest(
            Days: new List<MessagingHoursDayRequest>());

        // Act
        var response = await Client.WithoutAuth().PutAsJsonAsync(
            "/api/v1/messaging/settings/hours",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/messaging/stats ────────────────────────────────────────

    [Fact]
    public async Task GetTriageStats_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/messaging/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTriageStats_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/messaging/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTriageStats_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync("/api/v1/messaging/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/messaging/templates ────────────────────────────────────

    [Fact]
    public async Task ListTemplates_Authenticated_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/messaging/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListTemplates_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/messaging/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/messaging/templates ───────────────────────────────────

    [Fact]
    public async Task CreateTemplate_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new CreateTemplateRequest(
            Name: "Vaccination Reminder",
            ContentEn: "Dear {owner}, your pet {pet} is due for vaccination.",
            ContentAr: "عزيزي {owner}، حيوانك الأليف {pet} مستحق للتطعيم.",
            Category: MessageCategory.Administrative);

        // Act
        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/templates",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTemplate_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new CreateTemplateRequest("Test", "en", "ar", null);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/messaging/templates",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTemplate_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();
        var request = new CreateTemplateRequest("Test", "en", "ar", null);

        // Act
        var response = await receptionistClient.PostAsJsonAsync(
            "/api/v1/messaging/templates",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── PUT /api/v1/messaging/templates/{id} ───────────────────────────────

    [Fact]
    public async Task UpdateTemplate_Admin_ExistingTemplate_ReturnsSuccess()
    {
        // Arrange — create a template first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateTemplateRequest(
            "Surgery Follow-up",
            "Dear {owner}, how is {pet} recovering?",
            "عزيزي {owner}، كيف حال {pet}؟",
            MessageCategory.PostOperativeFollowUp);
        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/templates", createRequest, JsonOpts);
        var created = await createResponse.Content
            .ReadFromJsonAsync<ResponseTemplateDto>(JsonOpts);

        var updateRequest = new UpdateTemplateRequest(
            "Surgery Follow-up Updated",
            "Dear {owner}, how is {pet} doing after surgery?",
            "عزيزي {owner}، كيف حال {pet} بعد العملية؟",
            MessageCategory.PostOperativeFollowUp);

        // Act
        var response = await adminClient.PutAsJsonAsync(
            $"/api/v1/messaging/templates/{created!.Id}",
            updateRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateTemplate_Unauthenticated_Returns401()
    {
        // Arrange
        var updateRequest = new UpdateTemplateRequest("Test", "en", "ar", null);

        // Act
        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/messaging/templates/{Guid.NewGuid()}",
            updateRequest,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/messaging/templates/{id} ────────────────────────────

    [Fact]
    public async Task DeleteTemplate_Admin_ExistingTemplate_ReturnsSuccess()
    {
        // Arrange — create a template first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateTemplateRequest(
            "Temporary Template",
            "To be deleted",
            "سيتم حذفها",
            null);
        var createResponse = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/templates", createRequest, JsonOpts);
        var created = await createResponse.Content
            .ReadFromJsonAsync<ResponseTemplateDto>(JsonOpts);

        // Act
        var response = await adminClient.DeleteAsync(
            $"/api/v1/messaging/templates/{created!.Id}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTemplate_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().DeleteAsync(
            $"/api/v1/messaging/templates/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTemplate_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.DeleteAsync(
            $"/api/v1/messaging/templates/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
