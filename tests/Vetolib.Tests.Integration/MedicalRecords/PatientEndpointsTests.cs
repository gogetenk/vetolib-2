using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MedicalRecords;

/// <summary>
/// Integration tests for Patient and MedicalRecord endpoints.
/// </summary>
public sealed class PatientEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public PatientEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/patients ──────────────────────────────────────────────

    [Fact]
    public async Task CreatePatient_ValidRequest_Returns201()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new CreatePatientRequest(
            Name: "Max",
            Species: Species.Dog,
            Breed: "Golden Retriever",
            BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
            OwnerName: "Ahmed Al-Rashid",
            OwnerPhone: "+971 50 123 4567");

        // Act
        var response = await vetClient.PostAsJsonAsync("/api/v1/patients", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Max");
        body.ClinicId.Should().Be(TestClinicId);
    }

    [Fact]
    public async Task CreatePatient_AsReceptionist_Returns403()
    {
        // Arrange — CreatePatient requires VetOrAdmin policy
        var receptionistClient = CreateReceptionistClient();
        var request = new CreatePatientRequest(
            "Luna", Species.Cat, "Siamese",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            "Fatima Hassan", "+971 55 987 6543");

        // Act
        var response = await receptionistClient.PostAsJsonAsync("/api/v1/patients", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/patients?name= ─────────────────────────────────────────

    [Fact]
    public async Task ListPatients_SearchByName_ReturnsFilteredResults()
    {
        // Arrange — create two patients
        var vetClient = CreateVetClient();

        var request1 = new CreatePatientRequest(
            "Rex", Species.Dog, "German Shepherd",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4)),
            "Saeed Al-Maktoum", "+971 50 111 2222");

        var request2 = new CreatePatientRequest(
            "Whiskers", Species.Cat, "Persian",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            "Mariam Al-Suwaidi", "+971 55 333 4444");

        await vetClient.PostAsJsonAsync("/api/v1/patients", request1, JsonOptions);
        await vetClient.PostAsJsonAsync("/api/v1/patients", request2, JsonOptions);

        // Act — search for "Rex"
        var response = await vetClient.GetAsync("/api/v1/patients?name=Rex");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PatientPagedResultDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Items.Should().Contain(p => p.Name == "Rex");
        result.Items.Should().NotContain(p => p.Name == "Whiskers");
    }

    // ── POST /api/v1/patients/{id}/records ────────────────────────────────

    [Fact]
    public async Task AddMedicalRecord_ValidRequest_Returns200()
    {
        // Arrange — create a patient first, then add a medical record
        var vetClient = CreateVetClient();

        var createPatientRequest = new CreatePatientRequest(
            "Buddy", Species.Dog, "Labrador",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            "Khalid Al-Falasi", "+971 50 555 6666");

        var createResponse = await vetClient.PostAsJsonAsync("/api/v1/patients", createPatientRequest, JsonOptions);
        var patient = await createResponse.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);

        var recordRequest = new AddMedicalRecordRequest(
            Diagnosis: "Mild fever, recommended rest",
            Treatment: "Paracetamol 10mg/kg, 2 days");

        // Act
        var response = await vetClient.PostAsJsonAsync(
            $"/api/v1/patients/{patient!.Id}/records",
            recordRequest,
            JsonOptions);

        // Assert — ToMinimalApiResult() returns 200 for Result.Success (not 201)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MedicalRecordDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Diagnosis.Should().Be("Mild fever, recommended rest");
    }

    // ── POST /api/v1/patients/import ──────────────────────────────────────

    [Fact]
    public async Task ImportPatients_ValidCsv_Returns200WithReport()
    {
        // Arrange
        var vetClient = CreateVetClient();
        const string csvContent =
            "PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone\r\n" +
            "Fido,Dog,Beagle,2020-06-15,Hassan Al-Zaabi,hassan@test.ae,+971 50 777 8888\r\n" +
            "Mia,Cat,Tabby,2021-09-10,Aisha Al-Rashid,aisha@test.ae,+971 55 999 0000\r\n";

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "patients.csv");

        // Act
        var response = await vetClient.PostAsync("/api/v1/patients/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<ImportReportDto>(JsonOptions);
        report.Should().NotBeNull();
        report!.Imported.Should().Be(2);
        report.Skipped.Should().Be(0);
        report.Errors.Should().BeEmpty();
    }
}
