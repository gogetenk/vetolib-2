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
[Scope(Feature = "Assistant Messaging Access")]
internal class AssistantAccessSteps
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

    public AssistantAccessSteps(ScenarioContext ctx)
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

    [Given(@"I am authenticated as a user with role ""Assistant""")]
    public async Task GivenIAmAuthenticatedAsAssistant()
    {
        await AuthenticateAsRole("assistant@test-messaging.ae", UserRole.Assistant);
    }

    [Given(@"there are conversations categorized as ""AppointmentRequest"" and ""Administrative""")]
    public async Task GivenThereAreNonMedicalConversations()
    {
        await SeedConversationAsAdmin("Book an appointment please", "AppointmentRequest");
        await SeedConversationAsAdmin("What are your opening hours?", "Administrative");
    }

    [Given(@"there are conversations categorized as ""MedicalUrgency"" and ""MedicalQuestion""")]
    public async Task GivenThereAreMedicalConversations()
    {
        await SeedConversationAsAdmin("My dog is not breathing", "MedicalUrgency");
        await SeedConversationAsAdmin("My cat is sneezing", "MedicalQuestion");
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I open the Messages inbox")]
    public async Task WhenIOpenTheMessagesInbox()
    {
        _response = await _client.GetAsync("/api/v1/messaging/conversations");
        _ctx.Set(_response, "LastResponse");
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I should see these conversations")]
    public async Task ThenIShouldSeeTheseConversations()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Assistant inbox should be accessible");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.EnumerateArray().Should().NotBeEmpty(
            "Assistant should see non-medical conversations");
    }

    [Then(@"I should NOT see a reply button")]
    public async Task ThenIShouldNotSeeReplyButton()
    {
        // The API response for assistants should not include action permissions for reply
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        foreach (var conversation in body.EnumerateArray())
        {
            if (conversation.TryGetProperty("allowedActions", out var actions))
            {
                var actionList = actions.EnumerateArray().Select(a => a.GetString()).ToList();
                actionList.Should().NotContain("Reply",
                    "Assistant should not have reply permission");
            }
        }
    }

    [Then(@"I should NOT see action buttons \(transfer, convert to appointment\)")]
    public async Task ThenIShouldNotSeeActionButtons()
    {
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        foreach (var conversation in body.EnumerateArray())
        {
            if (conversation.TryGetProperty("allowedActions", out var actions))
            {
                var actionList = actions.EnumerateArray().Select(a => a.GetString()).ToList();
                actionList.Should().NotContain("Transfer",
                    "Assistant should not have transfer permission");
                actionList.Should().NotContain("ConvertToAppointment",
                    "Assistant should not have convert-to-appointment permission");
            }
        }
    }

    [Then(@"I should NOT see these conversations")]
    public async Task ThenIShouldNotSeeMedicalConversations()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK, "Inbox endpoint should respond");
        var body = await _response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var categories = body.EnumerateArray()
            .Select(c => c.GetProperty("category").GetString())
            .ToList();

        categories.Should().NotContain("MedicalUrgency",
            "Assistant should NOT see MedicalUrgency conversations");
        categories.Should().NotContain("MedicalQuestion",
            "Assistant should NOT see MedicalQuestion conversations");
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

    private async Task SeedConversationAsAdmin(string body, string category)
    {
        // Temporarily use admin credentials to seed data, then restore assistant auth
        var currentAuth = _client.DefaultRequestHeaders.Authorization?.ToString();

        await AuthenticateAsRole("admin.seed@test-messaging.ae", UserRole.Admin);
        await _client.PostAsJsonAsync("/api/v1/messaging/conversations/outbound", new
        {
            Body = body,
            Category = category,
            OwnerEmail = "owner@test-messaging.ae"
        });

        // Restore assistant auth
        if (currentAuth is not null)
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", currentAuth.Replace("Bearer ", ""));
        else
            await AuthenticateAsRole("assistant@test-messaging.ae", UserRole.Assistant);
    }
}
