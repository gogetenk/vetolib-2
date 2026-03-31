using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Notifications.Contracts.Dtos;
using Vetolib.Notifications.Contracts.Enums;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Notifications;

/// <summary>
/// Integration tests for Reminder configuration endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class ReminderEndpointsTests : IntegrationTestBase
{
    public ReminderEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/notifications/reminders/config ────────────────────────

    [Fact]
    public async Task GetReminderConfig_Authenticated_ReturnsSuccess()
    {
        var adminClient = CreateAdminClient();
        var response = await adminClient.GetAsync("/api/v1/notifications/reminders/config");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReminderConfig_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/notifications/reminders/config");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/v1/notifications/reminders/config ────────────────────────

    [Fact]
    public async Task UpdateReminderConfig_Authenticated_ReturnsSuccess()
    {
        var adminClient = CreateAdminClient();
        var request = new ReminderConfigDto(
            Appointment24hEnabled: true,
            VaccinationDueEnabled: true,
            FollowUpEnabled: false,
            Appointment24hLeadTimeHours: 24,
            VaccinationDueLeadTimeDays: 7,
            PreferredReminderChannel: ReminderChannel.Email);

        var response = await adminClient.PutAsJsonAsync(
            "/api/v1/notifications/reminders/config", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateReminderConfig_Unauthenticated_Returns401()
    {
        var request = new ReminderConfigDto(
            true, true, false, 24, 7, ReminderChannel.Email);

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            "/api/v1/notifications/reminders/config", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
