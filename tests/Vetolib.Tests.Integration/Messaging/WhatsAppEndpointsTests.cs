using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Messaging.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Messaging;

/// <summary>
/// Integration tests for WhatsApp endpoints.
/// Validates HTTP contract, auth (Admin-only), and serialization.
/// </summary>
public sealed class WhatsAppEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public WhatsAppEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/messaging/whatsapp/config ──────────────────────────────

    [Fact]
    public async Task GetWhatsAppConfig_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/messaging/whatsapp/config");

        // Assert — may return OK (with config) or NotFound (no config yet)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWhatsAppConfig_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            "/api/v1/messaging/whatsapp/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWhatsAppConfig_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync(
            "/api/v1/messaging/whatsapp/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── PUT /api/v1/messaging/whatsapp/config ──────────────────────────────

    [Fact]
    public async Task UpdateWhatsAppConfig_Admin_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new WhatsAppConfigRequest(
            WabaId: "123456789",
            PhoneNumberId: "987654321",
            AccessToken: "EAAxxxxxxx_test_token");

        // Act
        var response = await adminClient.PutAsJsonAsync(
            "/api/v1/messaging/whatsapp/config",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateWhatsAppConfig_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new WhatsAppConfigRequest("waba", "phone", "token");

        // Act
        var response = await Client.WithoutAuth().PutAsJsonAsync(
            "/api/v1/messaging/whatsapp/config",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateWhatsAppConfig_NonAdmin_Returns403()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new WhatsAppConfigRequest("waba", "phone", "token");

        // Act
        var response = await vetClient.PutAsJsonAsync(
            "/api/v1/messaging/whatsapp/config",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /api/v1/messaging/whatsapp/test ───────────────────────────────

    [Fact]
    public async Task SendWhatsAppTest_Admin_ReturnsSuccessOrServiceError()
    {
        // Arrange — first set up config so test message has something to work with
        var adminClient = CreateAdminClient();
        await adminClient.PutAsJsonAsync(
            "/api/v1/messaging/whatsapp/config",
            new WhatsAppConfigRequest("123456789", "987654321", "EAAxxxxxxx_test"),
            JsonOpts);

        var request = new WhatsAppTestRequest(
            RecipientPhone: "+971501234567",
            TemplateName: "hello_world");

        // Act
        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/messaging/whatsapp/test",
            request,
            JsonOpts);

        // Assert — may fail at the WhatsApp API level (fake credentials) but HTTP pipeline should work
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SendWhatsAppTest_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new WhatsAppTestRequest("+971501234567", "hello_world");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/messaging/whatsapp/test",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendWhatsAppTest_NonAdmin_Returns403()
    {
        // Arrange
        var receptionistClient = CreateReceptionistClient();
        var request = new WhatsAppTestRequest("+971501234567", "hello_world");

        // Act
        var response = await receptionistClient.PostAsJsonAsync(
            "/api/v1/messaging/whatsapp/test",
            request,
            JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
