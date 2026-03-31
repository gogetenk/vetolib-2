using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for Feedback endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class FeedbackEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public FeedbackEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/appointments/{id}/feedback ───────────────────────────

    [Fact]
    public async Task SubmitFeedback_Authenticated_ReturnsNon5xx()
    {
        var adminClient = CreateAdminClient();
        var request = new SubmitVisitFeedbackRequest(
            Rating: 5,
            Comment: "Excellent service",
            IsPublic: true);

        // Use a non-existent appointment ID — should get a business error, not 5xx
        var response = await adminClient.PostAsJsonAsync(
            $"/api/v1/appointments/{Guid.NewGuid()}/feedback", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitFeedback_Unauthenticated_Returns401()
    {
        var request = new SubmitVisitFeedbackRequest(5, "Great", true);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/appointments/{Guid.NewGuid()}/feedback", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/feedback (Admin) ──────────────────────────────────────

    [Fact]
    public async Task ListFeedback_AsAdmin_Returns200()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/feedback");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListFeedback_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/feedback");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListFeedback_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var response = await receptionistClient.GetAsync("/api/v1/feedback");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/feedback/stats (Admin) ────────────────────────────────

    [Fact]
    public async Task GetFeedbackStats_AsAdmin_Returns200()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/feedback/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFeedbackStats_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/feedback/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
