using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for QR check-in endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class CheckInQrEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public CheckInQrEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/appointments/{id}/checkin-qr ──────────────────────────

    [Fact]
    public async Task GetCheckInQr_Authenticated_ReturnsNon5xx()
    {
        var adminClient = CreateAdminClient();

        // Non-existent appointment — should return a business error, not 5xx
        var response = await adminClient.GetAsync(
            $"/api/v1/appointments/{Guid.NewGuid()}/checkin-qr");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetCheckInQr_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/appointments/{Guid.NewGuid()}/checkin-qr");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/appointments/checkin ──────────────────────────────────

    [Fact]
    public async Task CheckInFromQr_Authenticated_ReturnsNon5xx()
    {
        var adminClient = CreateAdminClient();
        var request = new CheckInFromQrRequest(
            AppointmentId: Guid.NewGuid(),
            PatientName: "Max",
            OwnerName: "Ahmed Al-Rashid",
            ScheduledTime: DateTime.UtcNow.AddHours(1),
            ClinicId: TestClinicId,
            Signature: "invalid-signature");

        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/appointments/checkin", request, JsonOpts);

        // Should get a business error (bad signature / not found), not 5xx
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CheckInFromQr_Unauthenticated_Returns401()
    {
        var request = new CheckInFromQrRequest(
            Guid.NewGuid(), "Max", "Owner", DateTime.UtcNow,
            TestClinicId, "sig");

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/appointments/checkin", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
