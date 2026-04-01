using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Billing.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Billing;

/// <summary>
/// Integration tests for E-Reporting endpoints.
/// Tests HTTP contract, auth/authz, and serialization for the e-reporting API.
/// </summary>
public sealed class EReportingEndpointsTests : IntegrationTestBase
{
    public EReportingEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/billing/ereporting/submit ────────────────────────────

    [Fact]
    public async Task SubmitEReporting_AdminRole_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new SubmitEReportingRequest(
            PeriodStart: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            PeriodEnd: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/billing/ereporting/submit", request, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SubmitEReporting_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new SubmitEReportingRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/billing/ereporting/submit", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitEReporting_NonAdminRole_ReturnsForbidden()
    {
        // Arrange — Vet role should not have access (Admin only)
        var vetClient = CreateVetClient();
        var request = new SubmitEReportingRequest(
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/billing/ereporting/submit", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/billing/ereporting/periods ────────────────────────────

    [Fact]
    public async Task GetEReportingPeriods_AdminRole_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/billing/ereporting/periods");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetEReportingPeriods_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            "/api/v1/billing/ereporting/periods");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEReportingPeriods_NonAdminRole_ReturnsForbidden()
    {
        // Arrange
        var vetClient = CreateVetClient();

        // Act
        var response = await vetClient.GetAsync("/api/v1/billing/ereporting/periods");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
