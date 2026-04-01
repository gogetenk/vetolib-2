using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Medical Record Template CRUD endpoints.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// Complements TemplateEndpointsTests with update and delete coverage.
/// </summary>
public sealed class MedicalRecordTemplateEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public MedicalRecordTemplateEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // -- PUT /api/v1/medical-records/templates/{id} --

    [Fact]
    public async Task UpdateTemplate_AsVet_NonExistentId_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();
        var request = new UpdateMedicalRecordTemplateRequest(
            Name: "Updated Checkup",
            Category: TemplateCategory.Checkup,
            DiagnosisTemplate: "Updated diagnosis template",
            TreatmentTemplate: "Updated treatment template",
            NotesTemplate: "Updated notes",
            Species: Species.Dog,
            SortOrder: 2);

        var response = await vetClient.PutAsJsonAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}", request, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateTemplate_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();
        var request = new UpdateMedicalRecordTemplateRequest(
            "Test", TemplateCategory.General, "Diag", "Treat", "Notes", null);

        var response = await receptionistClient.PutAsJsonAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateTemplate_Unauthenticated_Returns401()
    {
        var request = new UpdateMedicalRecordTemplateRequest(
            "Test", TemplateCategory.General, "Diag", "Treat", "Notes", null);

        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}", request, JsonOpts);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- DELETE /api/v1/medical-records/templates/{id} --

    [Fact]
    public async Task DeleteTemplate_AsVet_NonExistentId_ReturnsNon5xx()
    {
        var vetClient = CreateVetClient();

        var response = await vetClient.DeleteAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.NotFound,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task DeleteTemplate_AsReceptionist_Returns403()
    {
        var receptionistClient = CreateReceptionistClient();

        var response = await receptionistClient.DeleteAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTemplate_Unauthenticated_Returns401()
    {
        var response = await Client.WithoutAuth().DeleteAsync(
            $"/api/v1/medical-records/templates/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // -- Full CRUD round-trip --

    [Fact]
    public async Task TemplateCrud_CreateThenUpdateThenDelete_Succeeds()
    {
        var vetClient = CreateVetClient();

        // Create
        var createRequest = new CreateMedicalRecordTemplateRequest(
            Name: "Dental Checkup",
            Category: TemplateCategory.Checkup,
            DiagnosisTemplate: "Dental examination performed",
            TreatmentTemplate: "Scaling and polishing",
            NotesTemplate: "Recommend annual dental check",
            Species: Species.Dog,
            SortOrder: 5);

        var createResponse = await vetClient.PostAsJsonAsync(
            "/api/v1/medical-records/templates", createRequest, JsonOpts);
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<MedicalRecordTemplateDto>(JsonOpts);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        // Update
        var updateRequest = new UpdateMedicalRecordTemplateRequest(
            Name: "Dental Checkup v2",
            Category: TemplateCategory.Checkup,
            DiagnosisTemplate: "Updated dental exam template",
            TreatmentTemplate: "Updated treatment",
            NotesTemplate: "Updated notes",
            Species: Species.Dog,
            SortOrder: 10);

        var updateResponse = await vetClient.PutAsJsonAsync(
            $"/api/v1/medical-records/templates/{created.Id}", updateRequest, JsonOpts);
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Delete
        var deleteResponse = await vetClient.DeleteAsync(
            $"/api/v1/medical-records/templates/{created.Id}");
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }
}
