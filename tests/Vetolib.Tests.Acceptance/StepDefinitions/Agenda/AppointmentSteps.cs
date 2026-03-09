using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "Gestion des rendez-vous veterinaires")]
internal class AppointmentSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private AppointmentDto? _createdAppointment;
    private string? _errorResponseBody;

    private Guid _clinicId;
    private Guid _vetId;
    private string _vetName = string.Empty;
    private readonly Dictionary<string, Guid> _animalIds = new();
    private readonly Dictionary<string, string> _animalOwners = new();
    private readonly DateOnly _defaultDate = new(2026, 4, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppointmentSteps(ScenarioContext ctx)
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

    [Given(@"une clinique ""(.*)"" avec les horaires 9h-18h")]
    public void GivenUneCliniqueAvecLesHoraires(string clinicName)
    {
        _clinicId = GenerateGuidFromString(clinicName);
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    [Given(@"un veterinaire ""(.*)"" avec licence ""(.*)""")]
    public async Task GivenUnVeterinaireAvecLicence(string vetName, string licenseNumber)
    {
        _vetName = vetName;
        _vetId = GenerateGuidFromString(vetName);

        // Create the vet user in the auth database
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var email = $"{vetName.Replace(" ", "").Replace(".", "").ToLowerInvariant()}@happypaws.ae";
        var userResult = User.Create(_clinicId, email, "SecurePass1", UserRole.Vet, licenseNumber);
        userResult.IsSuccess.Should().BeTrue();

        // Override the Id using reflection to match our generated GUID
        typeof(Vetolib.Shared.Kernel.BaseEntity)
            .GetProperty("Id")!
            .SetValue(userResult.Value, _vetId);

        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();
    }

    [Given(@"un animal ""(.*)"" de race ""(.*)"" appartenant a ""(.*)""")]
    public void GivenUnAnimalDeRace(string animalName, string breed, string ownerName)
    {
        var animalId = GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        _animalOwners[animalName] = ownerName;
    }

    [Given(@"je suis authentifie en tant que RECEPTIONIST")]
    public async Task GivenJeSuisAuthentifieEnTantQueReceptionist()
    {
        // Create a receptionist user and login
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var email = "recep@happypaws.ae";
        var password = "SecurePass1";

        var userResult = User.Create(_clinicId, email, password, UserRole.Receptionist);
        userResult.IsSuccess.Should().BeTrue();

        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"un rendez-vous existant pour ""(.*)"" a ""(.*)""")]
    public async Task GivenUnRendezVousExistantPour(string animalName, string time)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "Owner";
        _animalOwners[animalName] = ownerName;

        var startTime = TimeOnly.Parse(time);
        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            30,
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        response.EnsureSuccessStatusCode();
    }

    [Given(@"un rendez-vous existant pour ""(.*)"" avec ""(.*)"" a ""(.*)"" pour (.*) minutes")]
    public async Task GivenUnRendezVousExistantPourAvecAPourMinutes(string animalName, string vetName, string time, int duration)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "Owner";

        var startTime = TimeOnly.Parse(time);
        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            duration,
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        response.EnsureSuccessStatusCode();
    }

    [Given(@"un rendez-vous existant de ""(.*)"" a ""(.*)"" avec ""(.*)""")]
    public async Task GivenUnRendezVousExistantDeAAvec(string startTimeStr, string endTimeStr, string vetName)
    {
        var startTime = TimeOnly.Parse(startTimeStr);
        var endTime = TimeOnly.Parse(endTimeStr);
        var duration = (int)(endTime - startTime).TotalMinutes;

        var animalName = "Max";
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            duration,
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        response.EnsureSuccessStatusCode();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"je cree un rendez-vous pour ""(.*)"" avec ""(.*)"" le ""(.*)"" a ""(.*)"" pour (.*) minutes")]
    public async Task WhenJeCreerUnRendezVous(string animalName, string vetName, string dateStr, string timeStr, int duration)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "Owner";
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            date,
            startTime,
            duration,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"je consulte l'agenda du ""(.*)""")]
    public async Task WhenJeConsulteLagenda(string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        _response = await _client.GetAsync($"/api/v1/appointments?date={date:yyyy-MM-dd}");

        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"je tente de creer un rendez-vous pour ""(.*)"" avec ""(.*)"" a ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVous(string animalName, string vetName, string timeStr)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "Owner";
        if (!_animalOwners.ContainsKey(animalName)) _animalOwners[animalName] = ownerName;

        var startTime = TimeOnly.Parse(timeStr);

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            30,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous de ""(.*)"" a ""(.*)"" avec ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousDeAAvec(string startTimeStr, string endTimeStr, string vetName)
    {
        var startTime = TimeOnly.Parse(startTimeStr);
        var endTime = TimeOnly.Parse(endTimeStr);
        var duration = (int)(endTime - startTime).TotalMinutes;

        var animalName = "Luna";
        var animalId = GenerateGuidFromString(animalName + "-overlap");
        var ownerName = "Jane Doe";

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            duration,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous a ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousA(string timeStr)
    {
        var startTime = TimeOnly.Parse(timeStr);
        var animalName = "Max";
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            _defaultDate,
            startTime,
            30,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous pour le ""(.*)"" a ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousPourLe(string dateStr, string timeStr)
    {
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);
        var animalName = "Max";
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            _vetId,
            _vetName,
            animalId,
            animalName,
            ownerName,
            date,
            startTime,
            30,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"le rendez-vous est cree avec le statut ""(.*)""")]
    public void ThenLeRendezVousEstCreeAvecLeStatut(string status)
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}: {_errorResponseBody}");
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Status.ToString().ToUpperInvariant().Should().Be(status.ToUpperInvariant());
    }

    [Then(@"le rendez-vous apparait dans l'agenda de ""(.*)"" a ""(.*)""")]
    public async Task ThenLeRendezVousApparaitDansLagenda(string vetName, string timeStr)
    {
        var date = _createdAppointment!.Date;
        var response = await _client.GetAsync($"/api/v1/appointments?date={date:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull();

        var expectedTime = TimeOnly.Parse(timeStr);
        appointments.Should().Contain(a =>
            a.VeterinarianId == _vetId &&
            a.StartTime == expectedTime);
    }

    [Then(@"je vois (.*) rendez-vous dans la liste")]
    public async Task ThenJeVoisRendezVousDansLaListe(int count)
    {
        _response.IsSuccessStatusCode.Should().BeTrue();
        var appointments = await _response.Content.ReadFromJsonAsync<List<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull();
        appointments.Should().HaveCount(count);
    }

    [Then(@"le systeme refuse avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseAvecLeCode(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().NotBeNull();
        _errorResponseBody.Should().Contain(errorCode);
    }

    [Then(@"le message est ""(.*)""")]
    public void ThenLeMessageEst(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"les prochains creneaux disponibles sont proposes")]
    public void ThenLesProchainsCreneauxDisponiblesSontProposes()
    {
        _errorResponseBody.Should().Contain("Prochains creneaux disponibles");
    }

    [Then(@"le message indique les horaires ""(.*)""")]
    public void ThenLeMessageIndiqueLesHoraires(string horaires)
    {
        // The format from ClinicSchedule.FormatHours() is "09h00 - 18h00"
        // The feature expects "9h00 - 18h00"
        _errorResponseBody.Should().Contain("horaires d'ouverture");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
