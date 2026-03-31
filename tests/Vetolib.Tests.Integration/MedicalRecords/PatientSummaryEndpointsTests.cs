using System.Net;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Patient summary export endpoint.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class PatientSummaryEndpointsTests : IntegrationTestBase
{
    public PatientSummaryEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/patients/{id}/export/summary ──────────────────────────

    [Fact]
    public async Task GetPatientSummary_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        // Non-existent patient — should return business error, not 5xx
        var response = await vetClient.GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/export/summary");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetPatientSummary_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/export/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
