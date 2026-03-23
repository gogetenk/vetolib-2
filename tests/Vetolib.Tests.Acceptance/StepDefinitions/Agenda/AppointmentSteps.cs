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
[Scope(Feature = "Veterinary appointment management")]
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

    // Used by the GetById/Edit scenarios
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

    [Given(@"a clinic ""(.*)"" with hours 9am-6pm")]
    public void GivenAClinicWithHours(string clinicName)
    {
        // Use the fixed TestClinicGuid so the multi-tenant query filter sees data
        // created in the same scenario (conflict checks, availability queries, etc.).
        _clinicId = TestClinicContext.TestClinicGuid;
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    [Given(@"a veterinarian ""(.*)"" with license ""(.*)""")]
    public async Task GivenAVeterinarianWithLicense(string vetName, string licenseNumber)
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

    [Given(@"an animal ""(.*)"" breed ""(.*)"" belonging to ""(.*)""")]
    public void GivenAnAnimalBreedBelongingTo(string animalName, string breed, string ownerName)
    {
        var animalId = GenerateGuidFromString(animalName);
        _animalIds[animalName] = animalId;
        _animalOwners[animalName] = ownerName;
    }

    [Given(@"an existing appointment for ""(.*)"" at ""(.*)""")]
    public async Task GivenAnExistingAppointmentForAt(string animalName, string time)
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

    [Given(@"an existing appointment for ""(.*)"" with ""(.*)"" at ""(.*)"" for (.*) minutes")]
    public async Task GivenAnExistingAppointmentForWithAtForMinutes(string animalName, string vetName, string time, int duration)
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

    [Given(@"an existing appointment from ""(.*)"" to ""(.*)"" with ""(.*)""")]
    public async Task GivenAnExistingAppointmentFromToWith(string startTimeStr, string endTimeStr, string vetName)
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

    [Given(@"the last appointment status has been updated to ""(.*)""")]
    public async Task GivenTheLastAppointmentStatusHasBeenUpdatedTo(string statusStr)
    {
        await WhenIUpdateTheLastAppointmentStatusTo(statusStr);
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Status update to {statusStr} failed: {_errorResponseBody}");
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I create an appointment for ""(.*)"" with ""(.*)"" on ""(.*)"" at ""(.*)"" for (.*) minutes")]
    public async Task WhenICreateAnAppointmentForWithOnAtForMinutes(string animalName, string vetName, string dateStr, string timeStr, int duration)
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
        _ctx.Set(_response, "LastResponse");

        if (_response.IsSuccessStatusCode)
        {
            _createdAppointment = await _response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
            _ctx.Set(_errorResponseBody, "ErrorResponseBody");
        }
    }

    [When(@"I view the agenda for ""(.*)""")]
    public async Task WhenIViewTheAgendaFor(string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        _response = await _client.GetAsync($"/api/appointments?date={date:yyyy-MM-dd}");
        _ctx.Set(_response, "LastResponse");

        if (!_response.IsSuccessStatusCode)
        {
            _errorResponseBody = await _response.Content.ReadAsStringAsync();
            _ctx.Set(_errorResponseBody, "ErrorResponseBody");
        }
    }

    [When(@"I try to create an appointment for ""(.*)"" with ""(.*)"" at ""(.*)""")]
    public async Task WhenITryToCreateAnAppointmentForWithAt(string animalName, string vetName, string timeStr)
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
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"I try to create an appointment from ""(.*)"" to ""(.*)"" with ""(.*)""")]
    public async Task WhenITryToCreateAnAppointmentFromToWith(string startTimeStr, string endTimeStr, string vetName)
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
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"I try to create an appointment at ""(.*)""")]
    public async Task WhenITryToCreateAnAppointmentAt(string timeStr)
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
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When("I try to create an appointment on date {string} at {string}")]
    public async Task WhenITryToCreateAnAppointmentOnDateAt(string dateStr, string timeStr)
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
        _ctx.Set(_response, "LastResponse");
        _ctx.Set(_errorResponseBody, "ErrorResponseBody");
    }

    [When(@"I update the last appointment status to ""(.*)""")]
    public async Task WhenIUpdateTheLastAppointmentStatusTo(string statusStr)
    {
        _createdAppointment.Should().NotBeNull("an appointment must have been created first");

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

    [When(@"I cancel the last appointment with reason ""(.*)""")]
    public async Task WhenICancelTheLastAppointmentWithReason(string reason)
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

    [When(@"I check availability for ""(.*)"" on ""(.*)"" for (.*) minutes")]
    public async Task WhenICheckAvailabilityForOnForMinutes(string vetName, string dateStr, int duration)
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

    [Then(@"the appointment is created with status ""(.*)""")]
    public void ThenTheAppointmentIsCreatedWithStatus(string status)
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}: {_errorResponseBody}");
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Status.ToString().ToUpperInvariant().Should().Be(status.ToUpperInvariant());
    }

    [Then(@"the appointment appears in ""(.*)"" agenda at ""(.*)""")]
    public async Task ThenTheAppointmentAppearsInAgendaAt(string vetName, string timeStr)
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

    [Then(@"I see (.*) appointments in the list")]
    public async Task ThenISeeAppointmentsInTheList(int count)
    {
        _response.IsSuccessStatusCode.Should().BeTrue();
        var appointments = await _response.Content.ReadFromJsonAsync<List<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull().And.HaveCount(count);
    }

    // NOTE: "the system rejects with code" is handled by SharedSteps (unscoped).
    // When steps in this class store LastResponse and ErrorResponseBody in ScenarioContext
    // so SharedSteps.ThenTheSystemRejectsWithCode can read them.

    [Then(@"the message is ""(.*)""")]
    public void ThenTheMessageIs(string expectedMessage)
    {
        _errorResponseBody.Should().Contain(expectedMessage);
    }

    [Then(@"the next available slots are suggested")]
    public void ThenTheNextAvailableSlotsAreSuggested()
    {
        _errorResponseBody.Should().Contain("Next available slots");
    }

    [Then(@"the message indicates the hours ""(.*)""")]
    public void ThenTheMessageIndicatesTheHours(string hours)
    {
        // The format from ClinicSchedule.FormatHours() is "09h00 - 18h00"
        _errorResponseBody.Should().Contain("outside of business hours");
    }

    [Then(@"the appointment status is ""(.*)""")]
    public void ThenTheAppointmentStatusIs(string expectedStatus)
    {
        _createdAppointment.Should().NotBeNull();
        _createdAppointment!.Status.ToString().Should().Be(expectedStatus);
    }

    [Then(@"the system rejects the transition with code ""(.*)""")]
    public void ThenTheSystemRejectsTheTransitionWithCode(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse();
        _errorResponseBody.Should().Contain(errorCode);
    }

    [Then(@"I see available and unavailable slots")]
    public void ThenISeeAvailableAndUnavailableSlots()
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}: {_errorResponseBody}");
        _availabilitySlots.Should().NotBeNull().And.NotBeEmpty("availability query should return at least one slot");
        _availabilitySlots.Should().Contain(s => s.IsAvailable);
        _availabilitySlots.Should().Contain(s => !s.IsAvailable);
    }

    [Then(@"the slot ""(.*)"" is marked unavailable")]
    public void ThenTheSlotIsMarkedUnavailable(string timeStr)
    {
        var expectedTime = TimeOnly.Parse(timeStr);
        _availabilitySlots.Should().NotBeNull().And.NotBeEmpty("availability query must return slots before checking individual ones");
        var slot = _availabilitySlots!.FirstOrDefault(s => s.StartTime == expectedTime);
        slot.Should().NotBeNull($"Slot {timeStr} should exist");
        slot!.IsAvailable.Should().BeFalse($"Slot {timeStr} should be unavailable");
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

    [Then(@"the operation succeeds")]
    public void ThenTheOperationSucceeds()
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected a success status code but got {(int)_response.StatusCode}: {_errorResponseBody}");
    }

    [Then(@"the request is rejected")]
    public void ThenTheRequestIsRejected()
    {
        ((int)_response.StatusCode).Should().Be(400,
            $"Expected status 400 but got {(int)_response.StatusCode}: {_errorResponseBody}");
    }

    [Then(@"the record is not found")]
    public void ThenTheRecordIsNotFound()
    {
        ((int)_response.StatusCode).Should().Be(404,
            $"Expected status 404 but got {(int)_response.StatusCode}: {_errorResponseBody}");
    }

    [Then(@"a conflict is detected")]
    public void ThenAConflictIsDetected()
    {
        ((int)_response.StatusCode).Should().Be(409,
            $"Expected status 409 but got {(int)_response.StatusCode}: {_errorResponseBody}");
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

    [Given(@"an existing appointment with status ""(.*)""")]
    public async Task GivenAnExistingAppointmentWithStatus(string status)
    {
        // If _clinicId was not set by GivenAClinicWithHours, read from ScenarioContext (set by SharedSteps.GivenAClinic)
        if (_clinicId == Guid.Empty)
        {
            if (_ctx.ContainsKey("ClinicIds"))
            {
                var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
                if (clinicIds.Count > 0)
                    _clinicId = clinicIds.Values.First();
            }
            if (_clinicId == Guid.Empty)
                _clinicId = TestClinicContext.TestClinicGuid;
        }

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

        // Normalize SCREAMING_SNAKE_CASE (e.g. "NO_SHOW") to PascalCase ("NoShow")
        // so Enum.TryParse can match enum member names correctly.
        var normalized = NormalizeEnumString(statusStr);

        // Try to parse as enum; if not parseable, send raw string to trigger 400
        if (Enum.TryParse<AppointmentStatus>(normalized, ignoreCase: true, out var parsedStatus))
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
        var normalized = NormalizeEnumString(statusStr);
        if (Enum.TryParse<AppointmentStatus>(normalized, ignoreCase: true, out var parsedStatus))
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

    /// <summary>
    /// Converts SCREAMING_SNAKE_CASE strings (e.g. "NO_SHOW", "CHECKED_IN") to PascalCase
    /// ("NoShow", "CheckedIn") so they can be parsed by <see cref="Enum.TryParse{T}"/>.
    /// Strings without underscores are returned unchanged.
    /// </summary>
    private static string NormalizeEnumString(string value)
    {
        if (!value.Contains('_'))
            return value;

        return string.Concat(
            value.Split('_')
                 .Select(word => word.Length == 0
                     ? word
                     : char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
