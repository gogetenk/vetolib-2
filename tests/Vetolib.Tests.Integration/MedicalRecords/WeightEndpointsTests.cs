using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Weight endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class WeightEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public WeightEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // -- POST /api/v1/patients/{patientId}/weights --

    [Fact]
    public async Task AddWeightEntry_AsVet_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();
        var request = new AddWeightEntryRequest(WeightKg: 12.5m, Note: "Annual checkup weight");

        var response = await vetClient.PostAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights", request, JsonOpts);

        // Non-existent patient -> business error, not 5xx
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddWeightEntry_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var request = new AddWeightEntryRequest(WeightKg: 12.5m);

        var response = await receptionistClient.PostAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddWeightEntry_Unauthenticated_Returns401()
    {
        var request = new AddWeightEntryRequest(WeightKg: 12.5m);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{patientId}/weights --

    [Fact]
    public async Task GetWeightHistory_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetWeightHistory_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/patients/{patientId}/weights/curve --

    [Fact]
    public async Task GetWeightCurve_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights/curve");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetWeightCurve_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/patients/{Guid.NewGuid()}/weights/curve");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
