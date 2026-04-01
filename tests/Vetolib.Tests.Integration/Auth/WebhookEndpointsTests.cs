using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Auth.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Webhook endpoints (/api/v1/webhooks/*).
/// Wiring tests: verifies HTTP contract, auth, and RBAC — no business logic.
/// </summary>
public sealed class WebhookEndpointsTests : IntegrationTestBase
{
    public WebhookEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/webhooks ───────────────────────────────────────────

    [Fact]
    public async Task RegisterWebhook_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();
        var request = new RegisterWebhookRequest(
            "appointment.created",
            "super-secret-key-123",
            new List<string> { "appointment.created", "appointment.cancelled" });

        var response = await client.PostAsJsonAsync("/api/v1/webhooks", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RegisterWebhook_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();
        var request = new RegisterWebhookRequest(
            "test-webhook",
            "secret",
            new List<string> { "test.event" });

        var response = await client.PostAsJsonAsync("/api/v1/webhooks", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterWebhook_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();
        var request = new RegisterWebhookRequest(
            "test-webhook",
            "secret",
            new List<string> { "test.event" });

        var response = await client.PostAsJsonAsync("/api/v1/webhooks", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/webhooks ────────────────────────────────────────────

    [Fact]
    public async Task ListWebhooks_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/webhooks");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ListWebhooks_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync("/api/v1/webhooks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListWebhooks_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.GetAsync("/api/v1/webhooks");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── DELETE /api/v1/webhooks/{id} ────────────────────────────────────

    [Fact]
    public async Task DeactivateWebhook_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.DeleteAsync($"/api/v1/webhooks/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateWebhook_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.DeleteAsync($"/api/v1/webhooks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeactivateWebhook_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.DeleteAsync($"/api/v1/webhooks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /api/v1/webhooks/receive ───────────────────────────────────

    [Fact]
    public async Task ReceiveWebhook_WithPayload_ReturnsNon5xx()
    {
        // This is a public endpoint (AllowAnonymous) — HMAC-verified
        var body = new
        {
            EventType = "appointment.created",
            Payload = JsonSerializer.SerializeToElement(new { id = Guid.NewGuid() })
        };

        var content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        // Add a dummy signature header
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks/receive")
        {
            Content = content
        };
        request.Headers.Add("X-Webhook-Signature", "dummy-signature");

        var response = await Client.SendAsync(request);

        // Should be 4xx (bad signature/validation), NOT 5xx (wiring broken)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }
}
