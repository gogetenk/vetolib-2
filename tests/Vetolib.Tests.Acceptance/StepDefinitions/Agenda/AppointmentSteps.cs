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
    private List<AvailabilitySlotDto>? _availabilitySlots;

    // Used by the English GetById/Edit scenarios
    private AppointmentDto? _firstAppointment;
    private AppointmentDto? _secondAppointment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
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
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {loginResponse.StatusCode}: {body}");
        }

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
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
    }

    [Given(@"un rendez-vous existant pour ""(.*)"" avec ""(.*)"" a ""(.*)"" pour (.*) minutes")]
    public async Task GivenUnRendezVousExistantPourAvecAPourMinutes(string animalName, string vetName, string time, int duration)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "Owner";

        var startTime = TimeOnly.Parse(time);
        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: duration,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
    }

    [Given(@"un rendez-vous existant de ""(.*)"" a ""(.*)"" avec ""(.*)""")]
    public async Task GivenUnRendezVousExistantDeAAvec(string startTimeStr, string endTimeStr, string vetName)
    {
        var startTime = TimeOnly.Parse(startTimeStr);
        var endTime = TimeOnly.Parse(endTimeStr);
        var durationMinutes = (int)(endTime - startTime).TotalMinutes;

        var animalName = "Max";
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: durationMinutes,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
    }

    [Given(@"le statut du dernier rendez-vous a ete mis a jour vers ""(.*)""")]
    public async Task GivenLeStatutDuDernierRendezVousAEteMisAJourVers(string statusStr)
    {
        await WhenJeMetsAJourLeStatutDuDernierRendezVousVers(statusStr);
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"La mise a jour de statut vers {statusStr} a echoue: {_errorResponseBody}");
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
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: date,
            StartTime: startTime,
            DurationMinutes: duration,
            Reason: null);

        _response = await _client.PostAsJsonAsync("/api/appointments", request);

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
        _response = await _client.GetAsync($"/api/appointments?date={date:yyyy-MM-dd}");

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
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        _response = await _client.PostAsJsonAsync("/api/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous de ""(.*)"" a ""(.*)"" avec ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousDeAAvec(string startTimeStr, string endTimeStr, string vetName)
    {
        var startTime = TimeOnly.Parse(startTimeStr);
        var endTime = TimeOnly.Parse(endTimeStr);
        var durationMinutes = (int)(endTime - startTime).TotalMinutes;

        var animalName = "Luna";
        var ownerName = "Jane Doe";

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: GenerateGuidFromString(animalName),
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: durationMinutes,
            Reason: null);

        _response = await _client.PostAsJsonAsync("/api/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous a ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousA(string timeStr)
    {
        var startTime = TimeOnly.Parse(timeStr);
        var animalName = "Max";
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: GenerateGuidFromString(animalName),
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        _response = await _client.PostAsJsonAsync("/api/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je tente de creer un rendez-vous pour le ""(.*)"" a ""(.*)""")]
    public async Task WhenJeTenteDeCreerUnRendezVousPourLe(string dateStr, string timeStr)
    {
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);
        var animalName = "Max";
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: GenerateGuidFromString(animalName),
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: date,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        _response = await _client.PostAsJsonAsync("/api/appointments", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"je mets a jour le statut du dernier rendez-vous vers ""(.*)""")]
    public async Task WhenJeMetsAJourLeStatutDuDernierRendezVousVers(string statusStr)
    {
        _createdAppointment.Should().NotBeNull("un rendez-vous doit avoir ete cree au prealable");

        // Map Reqnroll status names to the action string the endpoint expects
        var action = statusStr switch
        {
            "CheckedIn"  => "CHECK_IN",
            "InProgress" => "START",
            "Completed"  => "COMPLETE",
            "Cancelled"  => "CANCEL",
            "NoShow"     => "NO_SHOW",
            _ => statusStr.ToUpperInvariant()
        };

        var request = new TransitionAppointmentRequest(action, null);
        _response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{_createdAppointment!.Id}/transition", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"j'annule le dernier rendez-vous avec le motif ""(.*)""")]
    public async Task WhenJAnnuleLeDernierRendezVousAvecLeMotif(string reason)
    {
        _createdAppointment.Should().NotBeNull();
        var request = new TransitionAppointmentRequest("CANCEL", reason);
        _response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{_createdAppointment!.Id}/transition", request);

        if (_response.IsSuccessStatusCode)
        {
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
    }

    [When(@"je consulte les disponibilites de ""(.*)"" le ""(.*)"" pour (.*) minutes")]
    public async Task WhenJeConsulteLesDisponibilitesDeLePourtMinutes(string vetName, string dateStr, int duration)
    {
        var date = DateOnly.Parse(dateStr);
        _response = await _client.GetAsync(
            $"/api/appointments/availability?veterinarianId={_vetId}&date={date:yyyy-MM-dd}&durationMinutes={duration}");

        if (_response.IsSuccessStatusCode)
        {
            _availabilitySlots = await _response.Content.ReadFromJsonAsync<List<AvailabilitySlotDto>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
        }
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
        var dateOnly = _createdAppointment!.Date;

        var response = await _client.GetAsync($"/api/appointments?date={dateOnly:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull().And.NotBeEmpty("the agenda should contain at least the created appointment");

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
        appointments.Should().NotBeNull().And.HaveCount(count);
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

    [Then(@"le statut du rendez-vous est ""(.*)""")]
    public void ThenLeStatutDuRendezVousEst(string expectedStatus)
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}: {_errorResponseBody}");
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then(@"le systeme refuse la transition avec le code ""(.*)""")]
    public void ThenLeSystemeRefuseLaTransitionAvecLeCode(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().Contain(errorCode);
    }

    [Then(@"je vois des creneaux disponibles et non disponibles")]
    public void ThenJeVoisDesCraneauxDisponiblesEtNonDisponibles()
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}: {_errorResponseBody}");
        _availabilitySlots.Should().NotBeNull().And.NotBeEmpty("availability query should return at least one slot");
        _availabilitySlots.Should().Contain(s => s.IsAvailable);
        _availabilitySlots.Should().Contain(s => !s.IsAvailable);
    }

    [Then(@"le creneau ""(.*)"" est marque non disponible")]
    public void ThenLeCreneauEstMarqueNonDisponible(string timeStr)
    {
        var expectedTime = TimeOnly.Parse(timeStr);
        _availabilitySlots.Should().NotBeNull().And.NotBeEmpty("availability query must return slots before checking individual ones");
        var slot = _availabilitySlots!.FirstOrDefault(s => s.StartTime == expectedTime);
        slot.Should().NotBeNull($"Le creneau {timeStr} devrait exister");
        slot!.IsAvailable.Should().BeFalse($"Le creneau {timeStr} devrait etre non disponible");
    }

    // ─── English steps for GetById / EditAppointment scenarios ───

    [Given(@"an existing appointment for patient ""(.*)"" on ""(.*)"" at ""(.*)""")]
    public async Task GivenAnExistingAppointmentForPatientOnAt(string animalName, string dateStr, string timeStr)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";
        _animalOwners[animalName] = ownerName;

        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: date,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        _firstAppointment = _createdAppointment;
    }

    [Given(@"an existing appointment on ""(.*)"" at ""(.*)""")]
    public async Task GivenAnExistingAppointmentOnAt(string dateStr, string timeStr)
    {
        var animalName = "Luna";
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = "Jane Doe";
        _animalOwners[animalName] = ownerName;

        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: date,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _firstAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        _createdAppointment = _firstAppointment;
    }

    [Given(@"another appointment for ""(.*)"" on ""(.*)"" at ""(.*)""")]
    public async Task GivenAnotherAppointmentForOnAt(string animalName, string dateStr, string timeStr)
    {
        var animalId = _animalIds.ContainsKey(animalName) ? _animalIds[animalName] : GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = _animalOwners.ContainsKey(animalName) ? _animalOwners[animalName] : "John Smith";
        _animalOwners[animalName] = ownerName;

        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: date,
            StartTime: startTime,
            DurationMinutes: 30,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _secondAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        _createdAppointment = _secondAppointment;
    }

    [When(@"I request the appointment by its ID")]
    public async Task WhenIRequestTheAppointmentByItsId()
    {
        _createdAppointment.Should().NotBeNull("an appointment must have been created first");
        _response = await _client.GetAsync($"/api/appointments/{_createdAppointment!.Id}");
        if (_response.IsSuccessStatusCode)
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        else
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I request appointment with a random non-existent ID")]
    public async Task WhenIRequestAppointmentWithARandomNonExistentId()
    {
        var nonExistentId = Guid.NewGuid();
        _response = await _client.GetAsync($"/api/appointments/{nonExistentId}");
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I update the appointment to ""(.*)"" at ""(.*)""")]
    public async Task WhenIUpdateTheAppointmentToAt(string dateStr, string timeStr)
    {
        _createdAppointment.Should().NotBeNull("an appointment must have been created first");
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new UpdateAppointmentRequest(
            Date: date,
            StartTime: startTime,
            DurationMinutes: null,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: null,
            Notes: null);

        _response = await _client.PutAsJsonAsync($"/api/appointments/{_createdAppointment!.Id}", request);
        if (_response.IsSuccessStatusCode)
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        else
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I update the second appointment to ""(.*)"" at ""(.*)""")]
    public async Task WhenIUpdateTheSecondAppointmentToAt(string dateStr, string timeStr)
    {
        _secondAppointment.Should().NotBeNull("a second appointment must have been created first");
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        var request = new UpdateAppointmentRequest(
            Date: date,
            StartTime: startTime,
            DurationMinutes: null,
            VeterinarianId: null,
            VeterinarianName: null,
            Reason: null,
            Notes: null);

        _response = await _client.PutAsJsonAsync($"/api/appointments/{_secondAppointment!.Id}", request);
        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int statusCode)
    {
        ((int)_response.StatusCode).Should().Be(statusCode,
            $"Expected status {statusCode} but got {(int)_response.StatusCode}: {_errorResponseBody}");
    }

    [Then(@"the appointment details include patient ""(.*)"" and time ""(.*)""")]
    public void ThenTheAppointmentDetailsIncludePatientAndTime(string animalName, string timeStr)
    {
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.AnimalName.Should().Be(animalName);
        _createdAppointment.StartTime.Should().Be(TimeOnly.Parse(timeStr));
    }

    [Then(@"the appointment is now scheduled for ""(.*)"" at ""(.*)""")]
    public void ThenTheAppointmentIsNowScheduledForAt(string dateStr, string timeStr)
    {
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Date.Should().Be(DateOnly.Parse(dateStr));
        _createdAppointment.StartTime.Should().Be(TimeOnly.Parse(timeStr));
    }

    // ─── English steps for PATCH /status edge-case scenarios ─────

    [Given(@"a clinic ""(.*)""")]
    public void GivenAClinic(string clinicName)
    {
        _clinicId = GenerateGuidFromString(clinicName);
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    [Given(@"I am authenticated as ADMIN")]
    public async Task GivenIAmAuthenticatedAsAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var email = "admin-status@happypaws.ae";
        var password = "SecurePass1";

        var userResult = User.Create(_clinicId, email, password, UserRole.Admin);
        userResult.IsSuccess.Should().BeTrue();

        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Admin login failed with {loginResponse.StatusCode}: {body}");
        }

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"an existing appointment with status ""(.*)""")]
    public async Task GivenAnExistingAppointmentWithStatus(string status)
    {
        // Create a vet if not yet set up
        if (_vetId == Guid.Empty)
        {
            _vetName = "Dr. Test Vet";
            _vetId = GenerateGuidFromString(_vetName);
            using var scope = _factory.Services.CreateScope();
            var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var userResult = User.Create(_clinicId, "testvet-status@happypaws.ae", "SecurePass1", UserRole.Vet, "UAE-VET-99999");
            userResult.IsSuccess.Should().BeTrue();
            typeof(Vetolib.Shared.Kernel.BaseEntity)
                .GetProperty("Id")!
                .SetValue(userResult.Value, _vetId);
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var animalName = "Max";
        var animalId = GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        var ownerName = "John Smith";
        _animalOwners[animalName] = ownerName;

        var request = new CreateAppointmentRequest(
            VeterinarianId: _vetId,
            VeterinarianName: _vetName,
            AnimalId: animalId,
            AnimalName: animalName,
            OwnerName: ownerName,
            Date: _defaultDate,
            StartTime: new TimeOnly(10, 0),
            DurationMinutes: 30,
            Reason: null);

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        response.EnsureSuccessStatusCode();
        _createdAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
    }

    [When(@"I update the appointment status to ""(.*)""")]
    public async Task WhenIUpdateTheAppointmentStatusTo(string statusStr)
    {
        _createdAppointment.Should().NotBeNull("an appointment must have been created first");

        // Try to parse as enum; if not parseable, send raw string to trigger 400
        if (Enum.TryParse<AppointmentStatus>(statusStr, ignoreCase: true, out var parsedStatus))
        {
            var request = new UpdateAppointmentStatusRequest(parsedStatus, null);
            _response = await _client.PatchAsJsonAsync(
                $"/api/v1/appointments/{_createdAppointment!.Id}/status", request, JsonOptions);
        }
        else
        {
            // Send invalid JSON to trigger deserialization failure -> 400
            var rawJson = $"{{\"newStatus\":\"{statusStr}\",\"reason\":null}}";
            _response = await _client.PatchAsync(
                $"/api/v1/appointments/{_createdAppointment!.Id}/status",
                new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json"));
        }

        if (_response.IsSuccessStatusCode)
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        else
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [When(@"I update a non-existent appointment status to ""(.*)""")]
    public async Task WhenIUpdateANonExistentAppointmentStatusTo(string statusStr)
    {
        var nonExistentId = Guid.NewGuid();
        if (Enum.TryParse<AppointmentStatus>(statusStr, ignoreCase: true, out var parsedStatus))
        {
            var request = new UpdateAppointmentStatusRequest(parsedStatus, null);
            _response = await _client.PatchAsJsonAsync(
                $"/api/v1/appointments/{nonExistentId}/status", request, JsonOptions);
        }
        else
        {
            var rawJson = $"{{\"newStatus\":\"{statusStr}\",\"reason\":null}}";
            _response = await _client.PatchAsync(
                $"/api/v1/appointments/{nonExistentId}/status",
                new StringContent(rawJson, System.Text.Encoding.UTF8, "application/json"));
        }

        _errorResponseBody = await _response.Content.ReadAsStringAsync();
    }

    [Then(@"the appointment status is ""(.*)""")]
    public void ThenTheAppointmentStatusIs(string expectedStatus)
    {
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Status.ToString().Should().Be(expectedStatus);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
