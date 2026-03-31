using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for Waitlist endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class WaitlistEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public WaitlistEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/waitlist ─────────────────────────────────────────────

    [Fact]
    public async Task AddToWaitlist_Authenticated_ReturnsSuccessOrValidationError()
    {
        var adminClient = CreateAdminClient();
        var request = new CreateWaitlistEntryRequest(
            PatientId: Guid.NewGuid(),
            OwnerName: "Fatima Al-Zaabi",
            OwnerPhone: "+971 50 222 3333",
            OwnerEmail: "fatima@test.ae",
            PreferredDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            PreferredTimeSlot: PreferredTimeSlot.Morning,
            VetPreference: null,
            Reason: "Vaccination");

        var response = await adminClient.PostAsJsonAsync("/api/v1/waitlist", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddToWaitlist_Unauthenticated_Returns401()
    {
        var request = new CreateWaitlistEntryRequest(
            Guid.NewGuid(), "Owner", "+971 50 000 0000", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            PreferredTimeSlot.Any, null, null);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/waitlist", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/waitlist ──────────────────────────────────────────────

    [Fact]
    public async Task ListWaitlist_Authenticated_Returns200()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/waitlist");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListWaitlist_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/waitlist");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/waitlist/{id} ──────────────────────────────────────

    [Fact]
    public async Task RemoveFromWaitlist_NonExistentId_ReturnsNotFoundOrError()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.DeleteAsync($"/api/v1/waitlist/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RemoveFromWaitlist_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().DeleteAsync(
            $"/api/v1/waitlist/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
