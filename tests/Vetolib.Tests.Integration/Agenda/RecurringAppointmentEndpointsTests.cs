using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for recurring appointment series endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class RecurringAppointmentEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public RecurringAppointmentEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/appointments/series ──────────────────────────────────

    [Fact]
    public async Task CreateAppointmentSeries_Authenticated_ReturnsSuccessOrValidationError()
    {
        var adminClient = CreateAdminClient();
        var request = new CreateAppointmentSeriesRequest(
            VeterinarianId: Guid.NewGuid(),
            VeterinarianName: "Dr. Khalid Al-Fahim",
            AnimalId: Guid.NewGuid(),
            AnimalName: "Buddy",
            OwnerName: "Omar Al-Maktoum",
            OwnerEmail: "omar@test.ae",
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime: TimeOnly.Parse("10:00"),
            DurationMinutes: 30,
            Reason: "Follow-up checkup",
            Frequency: RecurrenceFrequency.Weekly,
            Count: 4);

        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/appointments/series", request, JsonOpts);

        // Accept 200/OK (series created) or 422 (validation error) — not 5xx
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointmentSeries_Unauthenticated_Returns401()
    {
        var request = new CreateAppointmentSeriesRequest(
            Guid.NewGuid(), "Dr. Test", Guid.NewGuid(), "Max", "Owner", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeOnly.Parse("10:00"), 30, null,
            RecurrenceFrequency.Weekly, 4);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/appointments/series", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/appointments/series/{id} ───────────────────────────

    [Fact]
    public async Task CancelAppointmentSeries_NonExistentId_ReturnsNotFoundOrError()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.DeleteAsync(
            $"/api/v1/appointments/series/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CancelAppointmentSeries_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().DeleteAsync(
            $"/api/v1/appointments/series/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
