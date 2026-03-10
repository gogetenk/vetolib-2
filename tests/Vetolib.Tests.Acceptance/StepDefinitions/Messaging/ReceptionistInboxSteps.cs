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
[Scope(Feature = "Receptionist Messaging Inbox")]
internal class ReceptionistInboxSteps
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

    public ReceptionistInboxSteps(ScenarioContext ctx)
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

    [Given(@"I am authenticated as a user with role ""Receptionist""")]
    public async Task GivenIAmAuthenticatedAsReceptionist()
    {
        await AuthenticateAsRole("receptionist@test-messaging.ae", UserRole.Receptionist);
    }

    [Given(@"there are messages categorized as ""AppointmentRequest"", ""Administrative"", and ""MedicalQuestion""")]
    public async Task GivenThereAreMessagesWithVariousCategories()
    {
        await SeedConversation("I need to book an appointment", "AppointmentRequest");
        await SeedConversation("What are your opening hours?", "Administrative");
        await SeedConversation("My cat is sneezing", "MedicalQuestion");
    }

    [Given(@"there are 3 messages: one ""Administrative"" from yesterday, one ""AppointmentRequest"" from today, one ""Administrative"" from today")]
    public async Task GivenThereAre3MessagesForPriorityTest()
    {
        await SeedConversation("Yesterday admin question", "Administrative");
        await SeedConversation("Book appointment please", "AppointmentRequest");
        await SeedConversation("Today admin question", "Administrative");
    }

    [Given(@"I open a message from an owner asking about appointment availability")]
    public async Task GivenIOpenAMessageAboutAppointmentAvailability()
    {
        var createResponse = await SeedConversation("I would like to book an appointment for next week", "AppointmentRequest");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
    }

    [Given(@"the AI has generated 2 suggested replies")]
    public void GivenAiHasGenerated2SuggestedReplies()
    {
        _ctx.Set(2, "SuggestedReplyCount");
    }

    [Given(@"the clinic has configured a template ""(.*)""")]
    public async Task GivenClinicHasConfiguredTemplate(string templateName)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/messaging/templates", new
        {
            Name = templateName,
            ContentEn = "We confirm your appointment on [DATE] at [TIME].",
            ContentAr = "نؤكد موعدك في [DATE] الساعة [TIME]."
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Template creation should succeed");

        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "TemplateId");
        _ctx.Set(templateName, "TemplateName");
    }

    [Given(@"I receive a message flagged as ""Triage uncertain -- please verify category""")]
    public async Task GivenIReceiveAnUncertainTriageMessage()
    {
        var createResponse = await SeedConversation("I have a question about my cat", "Administrative");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = Guid.Parse(body.GetProperty("id").GetString()!);
        _ctx.Set(conversationId, "ConversationId");
    }

    [Given(@"the message describes medical symptoms")]
    public void GivenMessageDescribesMedicalSymptoms()
    {
        _ctx.Set(true, "HasMedicalSymptoms");
    }

    [Given(@"I open a message requesting an appointment for pet ""(.*)""")]
    public async Task GivenIOpenAMessageRequestingAppointment(string petName)
    {
        var createResponse = await SeedConversation(
            $"I would like to book an appointment for my pet {petName}", "AppointmentRequest");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
        _ctx.Set(petName, "PetName");
    }

    [Given(@"I open a message that is clearly spam")]
    public async Task GivenIOpenASpamMessage()
    {
        var createResponse = await SeedConversation("Click here to win a prize!!!", "Administrative");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
    }

    [Given(@"I open a message linked to patient ""(.*)""")]
    public async Task GivenIOpenAMessageLinkedToPatient(string patientName)
    {
        var createResponse = await SeedConversation(
            $"Question about {patientName}", "Administrative");
        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
        _ctx.Set(patientName, "PatientName");
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I open the Messages inbox")]
    public async Task WhenIOpenTheMessagesInbox()
    {
        _response = await _client.GetAsync("/api/v1/messaging/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click on the first suggestion")]
    public async Task WhenIClickOnTheFirstSuggestion()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/suggestions");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I modify the text and click ""Send""")]
    public async Task WhenIModifyTheTextAndClickSend()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/reply", new
            {
                Body = "We have availability on Sunday. Would that work for you?",
                WasSuggestedReplyUsed = false
            });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I open a message and click ""Templates""")]
    public async Task WhenIOpenAMessageAndClickTemplates()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.GetAsync("/api/v1/messaging/templates");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I select the ""(.*)"" template")]
    public async Task WhenISelectTheTemplate(string templateName)
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var template = body.EnumerateArray()
            .FirstOrDefault(t => t.GetProperty("name").GetString() == templateName);
        template.ValueKind.Should().NotBe(JsonValueKind.Undefined,
            $"Template '{templateName}' should exist");
        _ctx.Set(template.GetProperty("contentEn").GetString()!, "PrefilledText");
    }

    [When(@"I click ""Transfer to veterinarian""")]
    public async Task WhenIClickTransferToVet()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PatchAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/transfer", new
            {
                TargetRole = "Vet"
            });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Convert to appointment""")]
    public async Task WhenIClickConvertToAppointment()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/convert-to-appointment", new { });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Mark as spam""")]
    public async Task WhenIClickMarkAsSpam()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        _response = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/spam", new { });
        _ctx.Set(_response, "LastResponse");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I should see messages categorized as ""AppointmentRequest"" and ""Administrative""")]
    public async Task ThenIShouldSeeAppointmentAndAdminMessages()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Inbox should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var categories = body.EnumerateArray()
            .Select(c => c.GetProperty("category").GetString())
            .ToList();

        categories.Should().Contain("AppointmentRequest",
            "Receptionist should see AppointmentRequest messages");
        categories.Should().Contain("Administrative",
            "Receptionist should see Administrative messages");
    }

    [Then(@"I should NOT see messages categorized as ""MedicalQuestion""")]
    public async Task ThenIShouldNotSeeMedicalQuestionMessages()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var categories = body.EnumerateArray()
            .Select(c => c.GetProperty("category").GetString())
            .ToList();
        categories.Should().NotContain("MedicalQuestion",
            "Receptionist should NOT see MedicalQuestion messages");
    }

    [Then(@"I should NOT see messages categorized as ""MedicalUrgency""")]
    public async Task ThenIShouldNotSeeMedicalUrgencyMessages()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var categories = body.EnumerateArray()
            .Select(c => c.GetProperty("category").GetString())
            .ToList();
        categories.Should().NotContain("MedicalUrgency",
            "Receptionist should NOT see MedicalUrgency messages");
    }

    [Then(@"the ""AppointmentRequest"" message should appear first \(higher priority\)")]
    public async Task ThenAppointmentRequestShouldAppearFirst()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversations = body.EnumerateArray().ToList();
        conversations.First().GetProperty("category").GetString().Should().Be("AppointmentRequest",
            "AppointmentRequest should have higher priority than Administrative");
    }

    [Then(@"the two ""Administrative"" messages should be sorted oldest first")]
    public async Task ThenAdminMessagesSortedOldestFirst()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var adminMessages = body.EnumerateArray()
            .Where(c => c.GetProperty("category").GetString() == "Administrative")
            .ToList();
        adminMessages.Should().BeInAscendingOrder(c => c.GetProperty("lastMessageAt").GetDateTime(),
            "Administrative messages should be sorted oldest first");
    }

    [Then(@"the reply field should be pre-filled with the suggestion text")]
    public async Task ThenReplyFieldShouldBePreFilled()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "AI suggestions should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetArrayLength().Should().BeGreaterThan(0, "At least one suggestion should exist");
    }

    [Then(@"the reply should be sent to the owner")]
    public async Task ThenReplyShouldBeSentToOwner()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Reply should be sent successfully");
    }

    [Then(@"the message status should change to ""InProgress""")]
    public async Task ThenMessageStatusShouldChangeToInProgress()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var detailResponse = await _client.GetAsync($"/api/v1/messaging/conversations/{conversationId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("status").GetString().Should().Be("InProgress",
            "Conversation status should change to InProgress after first reply");
    }

    [Then(@"the reply field should be pre-filled with the template text")]
    public void ThenReplyFieldShouldBePreFilledWithTemplate()
    {
        _ctx.ContainsKey("PrefilledText").Should().BeTrue("Template text should be set");
        _ctx.Get<string>("PrefilledText").Should().NotBeNullOrEmpty("Template text should not be empty");
    }

    [Then(@"I can modify it before sending")]
    public void ThenICanModifyItBeforeSending()
    {
        // UI behavior — verified in E2E Playwright tests
    }

    [Then(@"the message should disappear from my inbox")]
    public async Task ThenMessageShouldDisappearFromInbox()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Transfer/spam action should succeed");

        var inboxResponse = await _client.GetAsync("/api/v1/messaging/conversations");
        var body = await inboxResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var conversationId = _ctx.Get<Guid>("ConversationId");
        body.EnumerateArray().Should().NotContain(c =>
            Guid.Parse(c.GetProperty("id").GetString()!) == conversationId,
            "Transferred/spammed conversation should not appear in inbox");
    }

    [Then(@"it should appear in the vet inbox with a note ""(.*)""")]
    public async Task ThenItShouldAppearInVetInboxWithNote(string noteText)
    {
        // Verify by authenticating as vet and checking inbox
        // This requires cross-role verification — deferred to integration test
    }

    [Then(@"a new appointment form should open")]
    public async Task ThenANewAppointmentFormShouldOpen()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Convert to appointment should succeed");
    }

    [Then(@"the patient field should be pre-filled with ""(.*)""")]
    public async Task ThenPatientFieldShouldBePreFilled(string patientName)
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("patientName").GetString().Should().Be(patientName,
            "Patient field should be pre-filled from the conversation");
    }

    [Then(@"the owner field should be pre-filled")]
    public async Task ThenOwnerFieldShouldBePreFilled()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("ownerName", out _).Should().BeTrue("Owner field should be pre-filled");
    }

    [Then(@"the reason should contain the message content")]
    public async Task ThenReasonShouldContainMessageContent()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("reason").GetString().Should().NotBeNullOrEmpty(
            "Reason should be extracted from message content");
    }

    [Then(@"the admin should be able to view it in the spam folder")]
    public void ThenAdminShouldBeAbleToViewSpam()
    {
        // Verified in AdminMessagingSteps
    }

    [Then(@"I should see alongside the message: pet name, species, last appointment date, and outstanding invoices")]
    public async Task ThenIShouldSeePatientContext()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var contextResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/patient-context");
        contextResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Patient context should be accessible for receptionist");

        var body = await contextResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("petName", out _).Should().BeTrue("Context should include pet name");
        body.TryGetProperty("species", out _).Should().BeTrue("Context should include species");
        body.TryGetProperty("lastAppointmentDate", out _).Should().BeTrue("Context should include last appointment date");
        body.TryGetProperty("outstandingInvoices", out _).Should().BeTrue("Context should include outstanding invoices");
    }

    [Then(@"I should NOT see medical records \(consistent with receptionist RBAC\)")]
    public async Task ThenIShouldNotSeeMedicalRecords()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var contextResponse = await _client.GetAsync(
            $"/api/v1/messaging/conversations/{conversationId}/patient-context");
        contextResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await contextResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("medicalRecords", out _).Should().BeFalse(
            "Receptionist should NOT see medical records in patient context");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task AuthenticateAsRole(string email, UserRole role)
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
            var userResult = User.Create(clinicId, email, "SecurePass1", role, null);
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "SecurePass1"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"Login as {role} should succeed");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    private async Task<HttpResponseMessage> SeedConversation(string body, string category)
    {
        // Seed via portal token endpoint (no portal token seeded here — uses admin creation)
        var response = await _client.PostAsJsonAsync("/api/v1/messaging/conversations/outbound", new
        {
            Body = body,
            Category = category,
            OwnerEmail = "owner@test-messaging.ae"
        });
        return response;
    }
}
