using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Drug Catalog endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class DrugCatalogEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public DrugCatalogEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // -- GET /api/v1/medical-records/drugs --

    [Fact]
    public async Task SearchDrugs_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync("/api/v1/medical-records/drugs");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchDrugs_WithSearchQuery_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync("/api/v1/medical-records/drugs?search=amoxicillin&limit=5");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchDrugs_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/medical-records/drugs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/medical-records/drugs/{id} --

    [Fact]
    public async Task GetDrugById_Authenticated_NonExistentId_ReturnsNotFound()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync($"/api/v1/medical-records/drugs/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetDrugById_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/medical-records/drugs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- GET /api/v1/medical-records/drugs/{id}/alternatives --

    [Fact]
    public async Task GetDrugAlternatives_Authenticated_NonExistentId_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.GetAsync($"/api/v1/medical-records/drugs/{Guid.NewGuid()}/alternatives");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetDrugAlternatives_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/medical-records/drugs/{Guid.NewGuid()}/alternatives");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- POST /api/v1/medical-records/drugs (AddCustomDrug) --

    [Fact]
    public async Task AddCustomDrug_AsVet_ReturnsSuccess()
    {
        var vetClient = CreateVetClient();
        var request = new AddCustomDrugRequest(
            InnName: "Meloxicam",
            DisplayName: "Metacam 1.5mg/ml",
            Category: DrugCategory.AntiInflammatory);

        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/medical-records/drugs", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddCustomDrug_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var request = new AddCustomDrugRequest(
            InnName: "Meloxicam",
            DisplayName: "Metacam 1.5mg/ml",
            Category: DrugCategory.AntiInflammatory);

        var response = await receptionistClient.PostAsJsonAsync(
            "/api/v1/medical-records/drugs", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddCustomDrug_Unauthenticated_Returns401()
    {
        var request = new AddCustomDrugRequest(
            "Meloxicam", "Metacam 1.5mg/ml", DrugCategory.AntiInflammatory);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/medical-records/drugs", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- POST /api/v1/medical-records/prescriptions/preflight --

    [Fact]
    public async Task PrescriptionPreflight_Authenticated_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();
        var body = new { PatientId = Guid.NewGuid(), DrugCatalogEntryId = Guid.NewGuid(), DosageAmount = 5.0m };

        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/medical-records/prescriptions/preflight", body, JsonOpts);

        // Non-existent patient/drug -> business error, not 5xx
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PrescriptionPreflight_Unauthenticated_Returns401()
    {
        var body = new { PatientId = Guid.NewGuid(), DrugCatalogEntryId = Guid.NewGuid(), DosageAmount = 5.0m };

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/medical-records/prescriptions/preflight", body, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
