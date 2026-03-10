using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Patients;

[Binding]
[Scope(Feature = "CSV Import Patients")]
internal class CsvImportSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private ImportReportDto? _importReport;

    public CsvImportSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN steps ─────────────────────────────────────────────

    [Given(@"an owner with email ""(.*)"" already exists")]
    public async Task GivenOwnerWithEmailExists(string email)
    {
        var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
        var clinicId = clinicIds.Values.First();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var ownerResult = Owner.Create(clinicId, "Shared", "Owner", email, "+971 50 000 0001");
        ownerResult.IsSuccess.Should().BeTrue();
        db.Owners.Add(ownerResult.Value);
        await db.SaveChangesAsync();
    }

    // ─── WHEN steps ──────────────────────────────────────────────

    [When(@"I import a CSV with (\d+) valid patient rows?")]
    public async Task WhenImportCsvWithValidRows(int count)
    {
        var csvLines = new StringBuilder();
        csvLines.AppendLine("PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone");
        for (int i = 1; i <= count; i++)
        {
            csvLines.AppendLine($"Patient{i},Dog,Mixed,2020-01-{i:D2},Owner{i},owner{i}@test.ae,+971 50 {i:D3} 0000");
        }

        _response = await PostCsvAsync(csvLines.ToString());

        if (_response.IsSuccessStatusCode)
            _importReport = await _response.Content.ReadFromJsonAsync<ImportReportDto>(JsonOptions);
    }

    [When(@"I import a CSV where 1 row is missing Species")]
    public async Task WhenImportCsvMissingSpecies()
    {
        const string csv =
            "PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone\r\n" +
            "Rocky,Dog,Labrador,2021-05-10,Faisal,faisal@test.ae,+971 50 111 2222\r\n" +
            "BadRow,,Mixed,2020-01-01,BadOwner,badowner@test.ae,+971 50 000 0001\r\n";

        _response = await PostCsvAsync(csv);

        if (_response.IsSuccessStatusCode)
            _importReport = await _response.Content.ReadFromJsonAsync<ImportReportDto>(JsonOptions);
    }

    [When(@"I import a CSV with 2 patients sharing owner email ""(.*)""")]
    public async Task WhenImportCsvSharedEmail(string email)
    {
        var csv =
            "PatientName,Species,Breed,DateOfBirth,OwnerName,OwnerEmail,OwnerPhone\r\n" +
            $"PatA,Cat,Siamese,2021-01-01,Shared Owner,{email},+971 50 111 0001\r\n" +
            $"PatB,Cat,Persian,2022-06-15,Shared Owner,{email},+971 50 111 0001\r\n";

        _response = await PostCsvAsync(csv);

        if (_response.IsSuccessStatusCode)
            _importReport = await _response.Content.ReadFromJsonAsync<ImportReportDto>(JsonOptions);
    }

    [When(@"I request the CSV import template")]
    public async Task WhenRequestTemplate()
    {
        _response = await _client.GetAsync("/api/v1/patients/import/template");
    }

    // ─── THEN steps ──────────────────────────────────────────────

    [Then(@"the import report shows (\d+) imported, (\d+) skipped")]
    public void ThenImportReport(int expectedImported, int expectedSkipped)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        _importReport.Should().NotBeNull();
        _importReport!.Imported.Should().Be(expectedImported);
        _importReport.Skipped.Should().Be(expectedSkipped);
    }

    [Then(@"the imported patients appear in the patient list")]
    public async Task ThenImportedPatientsAppear()
    {
        var response = await _client.GetAsync("/api/v1/patients");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Patient1");
    }

    [Then(@"the errors list contains ""(.*)""")]
    public void ThenErrorsListContains(string expectedError)
    {
        _importReport.Should().NotBeNull();
        _importReport!.Errors.Should().Contain(e => e.Contains(expectedError));
    }

    [Then(@"only 1 owner exists with email ""(.*)""")]
    public async Task ThenOnlyOneOwnerWithEmail(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();
        var count = await db.Owners.IgnoreQueryFilters()
            .CountAsync(o => o.Email == email.ToLowerInvariant());
        count.Should().Be(1);
    }

    [Then(@"I receive a CSV file with header ""(.*)""")]
    public async Task ThenReceiveCsvWithHeader(string expectedHeader)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await _response.Content.ReadAsStringAsync();
        content.Should().StartWith(expectedHeader);
    }

    [Then(@"the import is rejected with status 403")]
    public void ThenImportRejectedWith403()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task<HttpResponseMessage> PostCsvAsync(string csvContent)
    {
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        form.Add(fileContent, "file", "patients.csv");
        return await _client.PostAsync("/api/v1/patients/import", form);
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
