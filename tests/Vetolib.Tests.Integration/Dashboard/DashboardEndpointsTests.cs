using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Dashboard;

/// <summary>
/// Integration tests for Dashboard endpoints.
/// Tests HTTP contracts, auth/authz, and serialization — not business logic.
/// Dashboard DTOs are internal to Vetolib.Api, so we deserialize into anonymous/JsonElement.
/// </summary>
public sealed class DashboardEndpointsTests : IntegrationTestBase
{
    public DashboardEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── Helper ────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds one appointment for today to ensure dashboard queries have data.
    /// </summary>
    private async Task SeedTodayAppointmentAsync(HttpClient adminClient)
    {
        var request = new CreateAppointmentRequest(
            VeterinarianId: Guid.NewGuid(),
            VeterinarianName: "Dr. Aisha Al-Mansoori",
            AnimalId: Guid.NewGuid(),
            AnimalName: "Luna",
            OwnerName: "Fatima Al-Zaabi",
            OwnerEmail: "fatima@email.ae",
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime: TimeOnly.Parse("09:00"),
            DurationMinutes: 30,
            Reason: "Vaccination");

        var response = await adminClient.PostAsJsonAsync("/api/v1/appointments", request, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/dashboard/stats ─────────────────────────────────────────

    [Fact]
    public async Task GetStats_Authenticated_Returns200WithExpectedShape()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        await SeedTodayAppointmentAsync(adminClient);

        // Act
        var response = await adminClient.GetAsync("/api/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        json.TryGetProperty("appointmentsToday", out _).Should().BeTrue();
        json.TryGetProperty("pendingCheckin", out _).Should().BeTrue();
        json.TryGetProperty("unpaidInvoicesAed", out _).Should().BeTrue();
        json.TryGetProperty("totalPatients", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetStats_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/dashboard/today-appointments ────────────────────────────

    [Fact]
    public async Task GetTodayAppointments_Authenticated_Returns200WithArray()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        await SeedTodayAppointmentAsync(adminClient);

        // Act
        var response = await adminClient.GetAsync("/api/dashboard/today-appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        json.ValueKind.Should().Be(JsonValueKind.Array);
        json.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);

        var first = json[0];
        first.TryGetProperty("id", out _).Should().BeTrue();
        first.TryGetProperty("patientName", out _).Should().BeTrue();
        first.TryGetProperty("ownerName", out _).Should().BeTrue();
        first.TryGetProperty("vetName", out _).Should().BeTrue();
        first.TryGetProperty("status", out _).Should().BeTrue();
        first.TryGetProperty("scheduledAt", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetTodayAppointments_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/dashboard/today-appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/dashboard/recent-activity ───────────────────────────────

    [Fact]
    public async Task GetRecentActivity_Authenticated_Returns200WithArray()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/dashboard/recent-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetRecentActivity_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/dashboard/recent-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/dashboard/analytics ─────────────────────────────────────

    [Fact]
    public async Task GetAnalytics_VetOrAdmin_Returns200WithExpectedShape()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/dashboard/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        json.TryGetProperty("revenueByMonth", out _).Should().BeTrue();
        json.TryGetProperty("noShowRate", out _).Should().BeTrue();
        json.TryGetProperty("patientsBySpecies", out _).Should().BeTrue();
        json.TryGetProperty("appointmentsByStatus", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetAnalytics_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/dashboard/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnalytics_Receptionist_Returns403()
    {
        // Arrange — receptionist does not have VetOrAdmin policy
        var receptionistClient = CreateReceptionistClient();

        // Act
        var response = await receptionistClient.GetAsync("/api/dashboard/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
