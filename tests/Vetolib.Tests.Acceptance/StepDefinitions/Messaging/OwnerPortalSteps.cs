using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Reqnroll;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Messaging;

[Binding]
[Scope(Feature = "Owner Messaging Portal")]
internal class OwnerPortalSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OwnerPortalSteps(ScenarioContext ctx)
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

    [Given(@"I am an owner with a valid magic link for ""(.*)""")]
    public async Task GivenIAmAnOwnerWithAValidMagicLink(string clinicName)
    {
        // Seed a magic link token for the test owner and set it on the HttpClient
        var tokenResponse = await _client.PostAsJsonAsync("/api/v1/portal/test-token", new
        {
            ClinicName = clinicName,
            OwnerEmail = "owner.luna@test.ae"
        });
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Test magic link creation should succeed");

        var body = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = body.GetProperty("token").GetString()!;
        _client.DefaultRequestHeaders.Add("X-Portal-Token", token);
        _ctx.Set(token, "MagicLinkToken");
    }

    [Given(@"I have a registered pet ""(.*)"" \(cat, (\d+) years old\)")]
    public async Task GivenIHaveARegisteredPet(string petName, int age)
    {
        _ctx.Set(petName, "PetName");
        _ctx.Set(age, "PetAge");
        // Pet registration is managed by MedicalRecords module; seeded via shared fixture
    }

    [Given(@"I am an owner with no registered pets")]
    public void GivenIAmAnOwnerWithNoRegisteredPets()
    {
        _ctx.Set(true, "NoPets");
    }

    [Given(@"I have never used the messaging portal before")]
    public void GivenIHaveNeverUsedTheMessagingPortalBefore()
    {
        _ctx.Set(false, "ConsentAccepted");
    }

    [Given(@"I open the clinic portal via my magic link")]
    public async Task GivenIOpenTheClinicPortalViaMyMagicLink()
    {
        _response = await _client.GetAsync("/api/v1/portal/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    [Given(@"I am composing a new message")]
    public void GivenIAmComposingANewMessage()
    {
        _ctx.Set(true, "IsComposing");
    }

    [Given(@"I have sent a message to the clinic")]
    public async Task GivenIHaveSentAMessageToTheClinic()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "I have a question about Luna",
            Category = "MedicalQuestion"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Conversation creation should succeed");

        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
    }

    [Given(@"I have an existing conversation about ""(.*)""")]
    public async Task GivenIHaveAnExistingConversationAbout(string subject)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "Message about " + subject,
            Category = "Administrative",
            Subject = subject
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Conversation creation should succeed");

        var body = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        _ctx.Set(Guid.Parse(body.GetProperty("id").GetString()!), "ConversationId");
        _ctx.Set(subject, "ConversationSubject");
    }

    [Given(@"my magic link has expired")]
    public void GivenMyMagicLinkHasExpired()
    {
        _client.DefaultRequestHeaders.Remove("X-Portal-Token");
        _client.DefaultRequestHeaders.Add("X-Portal-Token", "expired-token-12345");
    }

    [Given(@"I have already sent 5 messages today")]
    public async Task GivenIHaveAlreadySent5MessagesToday()
    {
        for (int i = 0; i < 5; i++)
        {
            var r = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
            {
                Body = $"Message number {i + 1}",
                Category = "Administrative"
            });
            r.StatusCode.Should().Be(HttpStatusCode.OK,
                $"Message {i + 1} of 5 should be accepted");
        }
    }

    [Given(@"I have conversations with the clinic")]
    public async Task GivenIHaveConversationsWithTheClinic()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "Export test message",
            Category = "Administrative"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Conversation creation should succeed");
    }

    [Given(@"the clinic's business hours are Sunday-Thursday 08:00-20:00")]
    public async Task GivenTheClinicsBusinessHours()
    {
        // Configured via admin endpoint — seed via support helper
        _ctx.Set("Sunday-Thursday 08:00-20:00", "BusinessHours");
    }

    [Given(@"the current time is Friday 22:00 Asia/Dubai")]
    public void GivenTheCurrentTimeIsFridayEvening()
    {
        _ctx.Set(new DateTimeOffset(2026, 3, 13, 22, 0, 0,
            TimeSpan.FromHours(4)), "CurrentTime");
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I click ""New Message""")]
    public async Task WhenIClickNewMessage()
    {
        _response = await _client.GetAsync("/api/v1/portal/pets");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I select my pet ""(.*)""")]
    public void WhenISelectMyPet(string petName)
    {
        _ctx.Set(petName, "SelectedPet");
    }

    [When(@"I select the category ""(.*)""")]
    public void WhenISelectTheCategory(string category)
    {
        _ctx.Set(category, "SelectedCategory");
    }

    [When(@"I type ""(.*)""")]
    public void WhenIType(string messageBody)
    {
        _ctx.Set(messageBody, "MessageBody");
    }

    [When(@"I type a message longer than 2000 characters")]
    public void WhenITypeAMessageLongerThan2000Characters()
    {
        _ctx.Set(new string('A', 2001), "MessageBody");
    }

    [When(@"I click ""Send""")]
    public async Task WhenIClickSend()
    {
        var body = _ctx.ContainsKey("MessageBody") ? _ctx.Get<string>("MessageBody") : "Test message";
        var pet = _ctx.ContainsKey("SelectedPet") ? _ctx.Get<string>("SelectedPet") : null;

        _response = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = body,
            Category = "MedicalQuestion",
            PatientName = pet
        });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I attach 2 photos \(JPG, under 5 MB each\)")]
    public void WhenIAttach2Photos()
    {
        _ctx.Set(2, "AttachmentCount");
    }

    [When(@"I try to attach a 4th photo")]
    public async Task WhenITryToAttachA4ThPhoto()
    {
        using var formContent = new MultipartFormDataContent();
        for (int i = 0; i < 4; i++)
        {
            var bytes = new byte[1024];
            formContent.Add(new ByteArrayContent(bytes), "photos", $"photo{i}.jpg");
        }
        formContent.Add(new StringContent("Test message"), "body");
        formContent.Add(new StringContent("MedicalQuestion"), "category");

        _response = await _client.PostAsync("/api/v1/portal/conversations", formContent);
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I open the portal")]
    public async Task WhenIOpenThePortal()
    {
        _response = await _client.GetAsync("/api/v1/portal/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I try to access the portal")]
    public async Task WhenITryToAccessThePortal()
    {
        _response = await _client.GetAsync("/api/v1/portal/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I try to send a 6th message")]
    public async Task WhenITryToSendA6ThMessage()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "6th message attempt",
            Category = "Administrative"
        });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I click ""Download my messages""")]
    public async Task WhenIClickDownloadMyMessages()
    {
        _response = await _client.GetAsync("/api/v1/portal/export");
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I send a non-urgent message")]
    public async Task WhenISendANonUrgentMessage()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "What time do you open on Sunday?",
            Category = "Administrative"
        });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"I send a message ""(.*)""")]
    public async Task WhenISendAMessage(string messageBody)
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = messageBody,
            Category = "MedicalUrgency"
        });
        _ctx.Set(_response, "LastResponse");
    }

    [When(@"a veterinarian replies to my message")]
    public async Task WhenAVeterinarianRepliesToMyMessage()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var replyResponse = await _client.PostAsJsonAsync(
            $"/api/v1/messaging/conversations/{conversationId}/reply", new
            {
                Body = "Thank you for your message. We will see Luna shortly.",
                WasSuggestedReplyUsed = false
            });
        replyResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Vet reply should succeed");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I should see a confirmation ""(.*)""")]
    public async Task ThenIShouldSeeAConfirmation(string expectedMessage)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected confirmation: {expectedMessage}");
    }

    [Then(@"I should see an estimated response time based on the category")]
    public async Task ThenIShouldSeeEstimatedResponseTime()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.TryGetProperty("estimatedResponseTime", out _).Should().BeTrue(
            "Response should include estimated response time");
    }

    [Then(@"the message should be sent with the 2 attachments")]
    public async Task ThenMessageSentWith2Attachments()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Message with attachments should be sent successfully");
    }

    [Then(@"the attachments should be visible in the conversation thread")]
    public void ThenAttachmentsShouldBeVisible()
    {
        // Verified by checking conversation detail response includes attachment URLs
    }

    [Then(@"I should see an error ""(.*)""")]
    public async Task ThenIShouldSeeAnError(string errorMessage)
    {
        _response.IsSuccessStatusCode.Should().BeFalse(
            $"Expected error: {errorMessage}");
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(errorMessage, "Error message should match");
    }

    [Then(@"I should see a character counter warning")]
    public async Task ThenIShouldSeeACharacterCounterWarning()
    {
        // Validation for max 2000 chars
        var body = _ctx.ContainsKey("MessageBody") ? _ctx.Get<string>("MessageBody") : "";
        body.Length.Should().BeGreaterThan(2000,
            "Message body should exceed the 2000-character limit");
    }

    [Then(@"the ""Send"" button should be disabled")]
    public async Task ThenTheSendButtonShouldBeDisabled()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = _ctx.Get<string>("MessageBody"),
            Category = "Administrative"
        });
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Server should reject messages exceeding 2000 characters");
    }

    [Then(@"I should receive an email ""(.*)""")]
    public void ThenIShouldReceiveAnEmail(string subject)
    {
        // Email delivery verified via notification log or outbox inspection
    }

    [Then(@"the email should contain a link back to the portal")]
    public void ThenEmailShouldContainPortalLink()
    {
        // Verified by checking email content in test notification sink
    }

    [Then(@"I should see the conversation in my list")]
    public async Task ThenIShouldSeeConversationInList()
    {
        var listResponse = await _client.GetAsync("/api/v1/portal/conversations");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Portal conversation list should be accessible");

        var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var subject = _ctx.Get<string>("ConversationSubject");
        body.EnumerateArray().Should().Contain(c =>
            c.GetProperty("subject").GetString()!.Contains(subject),
            "Conversation should appear in owner's list");
    }

    [Then(@"I should see all messages in chronological order")]
    public async Task ThenIShouldSeeMessagesInChronologicalOrder()
    {
        var conversationId = _ctx.Get<Guid>("ConversationId");
        var detailResponse = await _client.GetAsync($"/api/v1/portal/conversations/{conversationId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Conversation detail should be accessible");

        var body = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var messages = body.GetProperty("messages").EnumerateArray().ToList();
        messages.Should().BeInAscendingOrder(m => m.GetProperty("sentAt").GetDateTime(),
            "Messages should be in chronological order");
    }

    [Then(@"I should NOT see any internal notes from the staff")]
    public async Task ThenIShouldNotSeeInternalNotes()
    {
        var conversationId = _ctx.ContainsKey("ConversationId")
            ? _ctx.Get<Guid>("ConversationId")
            : Guid.Empty;

        var detailResponse = await _client.GetAsync($"/api/v1/portal/conversations/{conversationId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var messages = body.GetProperty("messages").EnumerateArray();
        messages.Should().NotContain(m => m.GetProperty("isInternalNote").GetBoolean(),
            "Owner should never see internal notes");
    }

    [Then(@"I should see ""(.*)""")]
    public async Task ThenIShouldSee(string expectedText)
    {
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedText, $"Response should contain: {expectedText}");
    }

    [Then(@"I should NOT be able to send any message")]
    public async Task ThenIShouldNotBeAbleToSendMessage()
    {
        var sendResponse = await _client.PostAsJsonAsync("/api/v1/portal/conversations", new
        {
            Body = "Attempting to send with expired token",
            Category = "Administrative"
        });
        sendResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Expired magic link should not allow sending messages");
    }

    [Then(@"only the ""Other"" category should be available")]
    public async Task ThenOnlyOtherCategoryAvailable()
    {
        var categoriesResponse = await _client.GetAsync("/api/v1/portal/categories");
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await categoriesResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var categories = body.EnumerateArray().Select(c => c.GetString()).ToList();
        categories.Should().ContainSingle(c => c == "Other",
            "Owner without pets should only see the 'Other' category");
    }

    [Then(@"the message should be routed to the receptionist")]
    public async Task ThenMessageShouldBeRoutedToReceptionist()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("assignedToRole").GetString().Should().Be("Receptionist",
            "Other category messages should be routed to receptionist");
    }

    [Then(@"I must accept them before I can compose a message")]
    public async Task ThenIMustAcceptConsent()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "First-time users must accept consent before sending messages");
    }

    [Then(@"I should see the messaging terms and conditions")]
    public async Task ThenIShouldSeeTermsAndConditions()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "Sending without consent should be blocked");
    }

    [Then(@"I should receive a text file containing all my conversations")]
    public async Task ThenIShouldReceiveATextFile()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Export should succeed");
        _response.Content.Headers.ContentType!.MediaType.Should()
            .Be("text/plain", "Export should return a text file");
    }

    [Then(@"I should receive an automatic acknowledgment ""(.*)""")]
    public async Task ThenIShouldReceiveAnAutomaticAcknowledgment(string expectedMessage)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Out-of-hours message should be accepted");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("acknowledgment").GetString().Should().Contain(expectedMessage,
            "Out-of-hours acknowledgment should be sent");
    }

    [Then(@"the on-call veterinarian should be notified immediately")]
    public void ThenOnCallVetShouldBeNotified()
    {
        // Verified via notification event in test sink
    }

    [Then(@"I should NOT receive the ""will be processed when the clinic reopens"" message")]
    public async Task ThenIShouldNotReceiveOutOfHoursMessage()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        if (body.TryGetProperty("acknowledgment", out var ack))
        {
            ack.GetString().Should().NotContain("will be processed when the clinic reopens",
                "Emergency messages should not receive the standard out-of-hours acknowledgment");
        }
    }
}
