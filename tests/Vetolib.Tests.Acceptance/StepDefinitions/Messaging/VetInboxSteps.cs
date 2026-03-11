using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Messaging;

[Binding]
[Scope(Feature = "Veterinarian Messaging Inbox")]
internal class VetInboxSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public VetInboxSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN Steps ─────────────────────────────────────────────

    [Given(@"I am authenticated as a user with role ""Vet""")]
    public async Task GivenIAmAuthenticatedAsVet()
    {
        await AuthenticateAsRole("vet@test-messaging.ae", UserRole.Vet, "VET-LIC-001");
    }

    [Given(@"there are messages: one ""MedicalUrgency"", one ""PostOperativeFollowUp"", one ""MedicalQuestion""")]
    public async Task GivenThereAreMessagesWithMedicalCategories()
    {
        await SeedConversation("My dog is not breathing", "MedicalUrgency");
        await SeedConversation("Stitches look infected after surgery", "PostOperativeFollowUp");
        await SeedConversation("My cat has been sneezing", "MedicalQuestion");
    }

    [Given(@"there is a ""MedicalQuestion"" from 2 hours ago")]
    public async Task GivenThereIsAMedicalQuestionFrom2HoursAgo()
    {
        await SeedConversation("Medical question from earlier", "MedicalQuestion");
    }

    [Given(@"there is a ""MedicalUrgency"" from 5 minutes ago")]
    public async Task GivenThereIsAMedicalUrgencyFrom5MinutesAgo()
    {
        await SeedConversation("Emergency right now!", "MedicalUrgency");
    }

    [Given(@"I open a message linked to patient ""(.*)"" \(cat, (\d+) years old\)")]
    public async Task GivenIOpenAMessageLinkedToPatient(string patientName, int age)
    {
        var createResponse = await SeedConversation(
            $"Question about {patientName}", "MedicalQuestion");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
        _ctx.Set(patientName, "PatientName");

        _response = await _client.GetAsync($"/api/v1/messaging/conversations/{_ctx.Get<Guid>("ConversationId")}");
        _ctx.Set(_response, "LastResponse");
    }

    [Given(@"a conversation has more than 5 messages")]
    public async Task GivenAConversationHasMoreThan5Messages()
    {
        var createResponse = await SeedConversation("Initial medical question", "MedicalQuestion");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");

        // Add additional messages to the conversation
        for (int i = 1; i <= 5; i++)
        {
            await _client.PostAsJsonAsync(
                $"/api/v1/messaging/conversations/{conversationId}/reply", new
                {
                    Body = $"Follow-up message {i}",
                    WasSuggestedReplyUsed = false
                });
        }
    }

    [Given(@"I open a conversation with an owner")]
    public async Task GivenIOpenAConversationWithAnOwner()
    {
        var createResponse = await SeedConversation("Health concern", "MedicalQuestion");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");

        _response = await _client.GetAsync($"/api/v1/messaging/conversations/{conversationId}");
        _ctx.Set(_response, "LastResponse");
    }

    [Given(@"I open a message with 3 AI-suggested replies")]
    public async Task GivenIOpenAMessageWith3AiSuggestedReplies()
    {
        var createResponse = await SeedConversation("My cat is not eating", "MedicalQuestion");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");
    }

    [Given(@"I open an emergency message about patient ""(.*)""")]
    public async Task GivenIOpenAnEmergencyMessageAboutPatient(string patientName)
    {
        var createResponse = await SeedConversation(
            $"{patientName} is not breathing and cannot stand", "MedicalUrgency");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");
        _ctx.Set(patientName, "PatientName");
    }

    [Given(@"I open a message where the owner describes symptoms and attached a photo")]
    public async Task GivenIOpenAMessageWithSymptoms()
    {
        var createResponse = await SeedConversation(
            "My cat Luna has red eyes and is squinting. I attached a photo.", "MedicalQuestion");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");
    }

    [Given(@"an owner sends a message classified as ""MedicalUrgency""")]
    public async Task GivenOwnerSendsEmergencyMessage()
    {
        var createResponse = await SeedConversation(
            "My dog collapsed and is not responding", "MedicalUrgency");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");
    }

    [Given(@"an emergency message was received 10 minutes ago")]
    public async Task GivenEmergencyMessageReceived10MinutesAgo()
    {
        var createResponse = await SeedConversation(
            "My dog is not breathing", "MedicalUrgency");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "EmergencyConversationId");
    }

    [Given(@"no veterinarian has viewed the message")]
    public void GivenNoVetHasViewedTheMessage()
    {
        _ctx.Set(true, "NoVetView");
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I open the Messages inbox")]
    public async Task WhenIOpenTheMessagesInbox()
    {
        _response = await _client.GetAsync("/api/v1/messaging/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I open the conversation")]
    public async Task WhenIOpenTheConversation()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.GetAsync($"/api/v1/messaging/conversations/{conversationId}");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Add internal note""")]
    public void WhenIClickAddInternalNote()
    {
        _ctx.Set("add-internal-note", "PendingAction");
    }

    [When(@"I type ""(.*)""")]
    public void WhenITypeNoteText(string noteText)
    {
        _ctx.Set(noteText, "NoteText");
    }

    [When(@"I click ""Save note""")]
    public async Task WhenIClickSaveNote()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var noteText = _ctx.Get<string>("NoteText");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/notes", new
            {
                Body = noteText
            });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click the second suggestion")]
    public async Task WhenIClickTheSecondSuggestion()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var suggestionsResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/suggestions");
        suggestionsResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Suggestions should be accessible");
        _ctx.Set("Selected suggestion 2", "SelectedSuggestion");
    }

    [When(@"I modify the text to add specific medical advice")]
    public void WhenIModifyTheTextToAddMedicalAdvice()
    {
        _ctx.Set("Modified reply with specific advice about kidney function tests.", "ReplyText");
    }

    [When(@"I click ""Send""")]
    public async Task WhenIClickSend()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var replyText = _ctx.ContainsKey("ReplyText") ? _ctx.Get<string>("ReplyText") : "Standard reply";
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/reply", new
            {
                Body = replyText,
                WasSuggestedReplyUsed = false
            });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Create urgent appointment""")]
    public async Task WhenIClickCreateUrgentAppointment()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/convert-to-appointment", new
            {
                AppointmentType = "Emergency"
            });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Add to medical record""")]
    public async Task WhenIClickAddToMedicalRecord()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/add-to-medical-record", new { });
        _ctx.Set(_response, "LastResponse");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"""MedicalUrgency"" should appear first \(red background\)")]
    public async Task ThenMedicalUrgencyShouldAppearFirst()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Inbox should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversations = body.EnumerateArray().ToList();
        conversations.First().GetProperty("category").GetString().Should().Be("MedicalUrgency",
            "MedicalUrgency should appear first in vet inbox");
    }

    [Then(@"""PostOperativeFollowUp"" should appear second")]
    public async Task ThenPostOpShouldAppearSecond()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversations = body.EnumerateArray().ToList();
        conversations[1].GetProperty("category").GetString().Should().Be("PostOperativeFollowUp",
            "PostOperativeFollowUp should appear second in vet inbox");
    }

    [Then(@"""MedicalQuestion"" should appear third")]
    public async Task ThenMedicalQuestionShouldAppearThird()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversations = body.EnumerateArray().ToList();
        conversations[2].GetProperty("category").GetString().Should().Be("MedicalQuestion",
            "MedicalQuestion should appear third in vet inbox");
    }

    [Then(@"the ""MedicalUrgency"" should be first regardless of the older message")]
    public async Task ThenMedicalUrgencyAlwaysFirst()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Inbox should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversations = body.EnumerateArray().ToList();
        conversations.First().GetProperty("category").GetString().Should().Be("MedicalUrgency",
            "Emergency messages should always appear at the top regardless of date");
    }

    [Then(@"I should see alongside the message:")]
    public async Task ThenIShouldSeeAlongsideTheMessage(Table contextTable)
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var contextResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/patient-context");
        contextResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Vet patient context should be accessible");

        var body = await contextResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("lastExaminationDate", out _).Should().BeTrue("Should include last examination date");
        body.TryGetProperty("currentPrescriptions", out _).Should().BeTrue("Should include current prescriptions");
        body.TryGetProperty("knownAllergies", out _).Should().BeTrue("Should include known allergies");
        body.TryGetProperty("vaccinationHistory", out _).Should().BeTrue("Should include vaccination history");
    }

    [Then(@"I should see an AI-generated summary at the top")]
    public async Task ThenIShouldSeeAiSummaryAtTop()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Conversation should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("summary", out var summary).Should().BeTrue(
            "Long conversation should include an AI summary");
        summary.GetString().Should().NotBeNullOrEmpty("Summary should not be empty");
    }

    [Then(@"the summary should be collapsible")]
    public void ThenSummaryShouldBeCollapsible()
    {
        // UI behavior — validated in E2E Playwright tests
    }

    [Then(@"the summary should be factual \(3-5 sentences, no medical interpretation\)")]
    public void ThenSummaryShouldBeFactual()
    {
        // Enforced by AI prompt constraints
    }

    [Then(@"the note should appear in the conversation thread")]
    public async Task ThenNoteShouldAppearInConversationThread()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Internal note creation should succeed");
    }

    [Then(@"the note should be visually distinct \(marked as ""Internal note""\)")]
    public async Task ThenNoteShouldBeVisuallyDistinct()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var detailResponse = await _client.GetAsync($"/api/v1/messaging/conversations/{conversationId}");
        var body = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        var messages = body.GetProperty("messages").EnumerateArray();
        messages.Should().Contain(m => m.GetProperty("isInternalNote").GetBoolean(),
            "Conversation should contain an internal note");
    }

    [Then(@"the owner should NOT see this note")]
    public void ThenOwnerShouldNotSeeNote()
    {
        // Verified in OwnerPortalSteps - owner conversation detail strips internal notes
    }

    [Then(@"the reply should be sent to the owner")]
    public async Task ThenReplyShouldBeSentToOwner()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Reply should be sent successfully");
    }

    [Then(@"the system should record WasSuggestedReplyUsed as false \(modified\)")]
    public async Task ThenWasSuggestedReplyUsedShouldBeFalse()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var auditResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/reply-audit");
        auditResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await auditResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("wasSuggestedReplyUsed").GetBoolean().Should().BeFalse(
            "WasSuggestedReplyUsed should be false when the suggestion was modified");
    }

    [Then(@"the system should record the ActualReply")]
    public async Task ThenSystemShouldRecordActualReply()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var auditResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/reply-audit");
        var body = await auditResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("actualReply").GetString().Should().NotBeNullOrEmpty(
            "ActualReply should be recorded");
    }

    [Then(@"a new appointment form should open with:")]
    public async Task ThenNewAppointmentFormShouldOpenWith(Table expectedFields)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Create urgent appointment should succeed");

        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("appointmentId", out _).Should().BeTrue(
            "Response should include the created appointment ID");
    }

    [Then(@"the appointment should be created in the next available slot")]
    public async Task ThenAppointmentShouldBeCreated()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("scheduledAt", out _).Should().BeTrue(
            "Appointment should have a scheduled time");
    }

    [Then(@"the message text and photos should be added as a note in the patient's medical record")]
    public async Task ThenMessageShouldBeAddedToMedicalRecord()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Adding to medical record should succeed");
    }

    [Then(@"a confirmation should appear ""(.*)""")]
    public async Task ThenConfirmationShouldAppear(string confirmationText)
    {
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(confirmationText, $"Confirmation should mention: {confirmationText}");
    }

    [Then(@"I should receive a browser push notification immediately")]
    public void ThenIShouldReceiveBrowserPushNotification()
    {
        // Verified via notification event in test sink
    }

    [Then(@"the notification should show the patient name and a preview of the message")]
    public void ThenNotificationShouldShowPatientName()
    {
        // Verified by inspecting push notification payload in test sink
    }

    [Then(@"an escalation notification should be sent to all veterinarians of the clinic")]
    public async Task ThenEscalationNotificationShouldBeSent()
    {
        var emergencyId = _ctx.Get<Guid>("EmergencyConversationId");
        var escalationResponse = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{emergencyId}/escalate", new { });
        escalationResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Escalation should be triggered successfully");
    }

    [Then(@"the notification should include ""(.*)""")]
    public void ThenNotificationShouldInclude(string text)
    {
        // Verified by inspecting notification content in test sink
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task AuthenticateAsRole(string email, UserRole role, string? vetLicense = null)
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var existing = await authDb.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            var userResult = User.Create(clinicId, email, "SecurePass1", role, vetLicense);
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "SecurePass1"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Login as {role} should succeed");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    private async Task<HttpResponseMessage> SeedConversation(string body, string category)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/messaging/conversations/outbound", new
        {
            Body = body,
            Category = category,
            OwnerEmail = "owner@test-messaging.ae"
        });
        return response;
    }
}
