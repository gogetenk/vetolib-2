using System.Net;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Owner Portal endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// Note: These endpoints require an owner_account_id claim. Standard vet/admin tokens
/// do not carry this claim, so the endpoint returns 403 (Forbidden) by design.
/// </summary>
public sealed class OwnerPortalEndpointsTests : IntegrationTestBase
{
    public OwnerPortalEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // -- GET /api/v1/portal/my-animals --

    [Fact]
    public async Task GetMyAnimals_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/portal/my-animals");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyAnimals_AuthenticatedWithoutOwnerClaim_Returns403()
    {
        // Vet tokens don't have owner_account_id claim -> endpoint returns Forbidden
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync("/api/v1/portal/my-animals");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -- GET /api/v1/portal/animals/{id}/records --

    [Fact]
    public async Task GetAnimalRecords_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/records");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnimalRecords_AuthenticatedWithoutOwnerClaim_Returns403()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/records");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -- GET /api/v1/portal/animals/{id}/vaccinations --

    [Fact]
    public async Task GetAnimalVaccinations_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/vaccinations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnimalVaccinations_AuthenticatedWithoutOwnerClaim_Returns403()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/vaccinations");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -- GET /api/v1/portal/animals/{id}/prescriptions --

    [Fact]
    public async Task GetAnimalPrescriptions_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/prescriptions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnimalPrescriptions_AuthenticatedWithoutOwnerClaim_Returns403()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/prescriptions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -- GET /api/v1/portal/animals/{id}/weight --

    [Fact]
    public async Task GetAnimalWeightHistory_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/weight");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnimalWeightHistory_AuthenticatedWithoutOwnerClaim_Returns403()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/portal/animals/{Guid.NewGuid()}/weight");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
