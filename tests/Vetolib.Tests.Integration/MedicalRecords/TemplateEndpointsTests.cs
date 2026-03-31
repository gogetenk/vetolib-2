using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Medical Record Template endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class TemplateEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TemplateEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/medical-records/templates ─────────────────────────────

    [Fact]
    public async Task ListTemplates_Authenticated_Returns200()
    {
        var vetClient = CreateVetClient();
        var response = await vetClient.GetAsync("/api/v1/medical-records/templates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListTemplates_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().GetAsync("/api/v1/medical-records/templates");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/medical-records/templates ────────────────────────────

    [Fact]
    public async Task CreateTemplate_AsVet_ReturnsSuccess()
    {
        var vetClient = CreateVetClient();
        var request = new CreateMedicalRecordTemplateRequest(
            Name: "Routine Checkup",
            Category: TemplateCategory.Checkup,
            DiagnosisTemplate: "Patient presents for routine examination",
            TreatmentTemplate: "General health assessment performed",
            NotesTemplate: "Next visit in 6 months",
            Species: Species.Dog,
            SortOrder: 1);

        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/medical-records/templates", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTemplate_Unauthenticated_Returns401()
    {
        var request = new CreateMedicalRecordTemplateRequest(
            "Test", TemplateCategory.General, "Diag", "Treat", "Notes", null);

        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/medical-records/templates", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
