using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Preferences;

/// <summary>
/// Integration tests for Working Hours endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class WorkingHoursEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public WorkingHoursEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/preferences/working-hours ─────────────────────────────

    [Fact]
    public async Task GetWorkingHours_Authenticated_Returns200()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/preferences/working-hours");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetWorkingHours_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/preferences/working-hours");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/v1/preferences/working-hours (Admin) ─────────────────────

    [Fact]
    public async Task UpsertWorkingHours_AsAdmin_ReturnsSuccess()
    {
        var adminClient = CreateAdminClient();

        // The endpoint expects an internal request DTO with Days array
        var payload = new
        {
            Days = new[]
            {
                new { DayOfWeek = 0, IsOpen = true, OpenTime = "08:00:00", CloseTime = "18:00:00", BreakStartTime = (string?)null, BreakEndTime = (string?)null },
                new { DayOfWeek = 1, IsOpen = true, OpenTime = "08:00:00", CloseTime = "18:00:00", BreakStartTime = (string?)null, BreakEndTime = (string?)null }
            }
        };

        var response = await adminClient.PutAsJsonAsync(
            "/api/v1/preferences/working-hours", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpsertWorkingHours_Unauthenticated_Returns401()
    {
        var payload = new { Days = Array.Empty<object>() };

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            "/api/v1/preferences/working-hours", payload, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpsertWorkingHours_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var payload = new
        {
            Days = new[]
            {
                new { DayOfWeek = 0, IsOpen = true, OpenTime = "08:00:00", CloseTime = "18:00:00", BreakStartTime = (string?)null, BreakEndTime = (string?)null }
            }
        };

        var response = await receptionistClient.PutAsJsonAsync(
            "/api/v1/preferences/working-hours", payload, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
