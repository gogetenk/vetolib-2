using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Portal;

[Binding]
[Scope(Feature = "Clinic Search")]
internal class ClinicSearchSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage? _response;
    private ClinicSearchPagedResultDto? _searchResult;
    private string? _errorResponseBody;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public ClinicSearchSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN Steps ─────────────────────────────────────────────

    [Given(@"the following clinics exist in the directory")]
    public async Task GivenTheFollowingClinicsExistInTheDirectory(DataTable table)
    {
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        foreach (var row in table.Rows)
        {
            var name = row["Name"];
            var city = row["City"];
            var speciesCsv = row["Supported Species"];
            var species = speciesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var clinicResult = Clinic.Create(name);
            clinicResult.IsSuccess.Should().BeTrue($"Clinic creation should succeed for '{name}'");

            var clinic = clinicResult.Value;
            clinic.UpdateDirectory(city, null, species);

            authDb.Clinics.Add(clinic);
        }

        await authDb.SaveChangesAsync();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I search for clinics with name ""(.*)""")]
    public async Task WhenISearchForClinicsWithName(string name)
    {
        _response = await _client.GetAsync($"/api/v1/clinics/search?name={Uri.EscapeDataString(name)}");
        await ParseSearchResult();
    }

    [When(@"I search for clinics in city ""(.*)""")]
    public async Task WhenISearchForClinicsInCity(string city)
    {
        _response = await _client.GetAsync($"/api/v1/clinics/search?city={Uri.EscapeDataString(city)}");
        await ParseSearchResult();
    }

    [When(@"I search for clinics that treat ""(.*)""")]
    public async Task WhenISearchForClinicsThatTreat(string species)
    {
        _response = await _client.GetAsync($"/api/v1/clinics/search?species={Uri.EscapeDataString(species)}");
        await ParseSearchResult();
    }

    [When(@"I search for clinics in city ""(.*)"" that treat ""(.*)""")]
    public async Task WhenISearchForClinicsInCityThatTreat(string city, string species)
    {
        _response = await _client.GetAsync(
            $"/api/v1/clinics/search?city={Uri.EscapeDataString(city)}&species={Uri.EscapeDataString(species)}");
        await ParseSearchResult();
    }

    [When(@"I search for all clinics with page size (\d+)")]
    public async Task WhenISearchForAllClinicsWithPageSize(int pageSize)
    {
        _response = await _client.GetAsync($"/api/v1/clinics/search?pageSize={pageSize}");
        await ParseSearchResult();
    }

    [When(@"I search for clinics without being logged in")]
    public async Task WhenISearchForClinicsWithoutBeingLoggedIn()
    {
        // Ensure no auth header is set
        _client.DefaultRequestHeaders.Authorization = null;
        _response = await _client.GetAsync("/api/v1/clinics/search");
        await ParseSearchResult();
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I should see (\d+) clinics? in the results")]
    public void ThenIShouldSeeNClinicsInTheResults(int count)
    {
        _searchResult.Should().NotBeNull();
        _searchResult!.Items.Should().HaveCount(count);
    }

    [Then(@"the results should include ""(.*)""")]
    public void ThenTheResultsShouldInclude(string clinicName)
    {
        _searchResult.Should().NotBeNull();
        _searchResult!.Items.Should().Contain(c => c.Name == clinicName);
    }

    [Then(@"the total count should be (\d+)")]
    public void ThenTheTotalCountShouldBe(int totalCount)
    {
        _searchResult.Should().NotBeNull();
        _searchResult!.TotalCount.Should().Be(totalCount);
    }

    [Then(@"the search should succeed")]
    public void ThenTheSearchShouldSucceed()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task ParseSearchResult()
    {
        if (_response is not null && _response.IsSuccessStatusCode)
        {
            _searchResult = await _response.Content
                .ReadFromJsonAsync<ClinicSearchPagedResultDto>(JsonOptions);
        }
        else if (_response is not null)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }
}
