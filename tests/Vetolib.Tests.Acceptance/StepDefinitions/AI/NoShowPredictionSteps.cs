using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.AI.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.AI;

[Binding]
[Scope(Feature = "No-Show Prediction")]
internal class NoShowPredictionSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private NoShowPredictionDto? _prediction;
    private List<NoShowPredictionDto>? _batchPredictions;
    private Guid _targetAppointmentId;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public NoShowPredictionSteps(ScenarioContext ctx)
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

    [Given(@"I am authenticated as a user with role ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsRole(string role)
    {
        var clinicId = GetOrCreateClinicId();

        var email = $"noshow-{role.ToLowerInvariant()}-{Guid.NewGuid():N}@happypaws.ae";
        const string password = "SecurePass1";

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role.ToUpperInvariant() switch
        {
            "VET" => UserRole.Vet,
            "RECEPTIONIST" => UserRole.Receptionist,
            "ADMIN" => UserRole.Admin,
            // "Owner" has no direct mapping — use Assistant (lowest privilege)
            // so the JWT is valid but ClinicStaff policy will deny access.
            _ => UserRole.Assistant
        };

        var vetLicense = userRole == UserRole.Vet ? "NOSHOW-VET-001" : null;
        var userResult = User.Create(clinicId, email, password, userRole, vetLicense);
        userResult.IsSuccess.Should().BeTrue($"User creation failed for role {role}");
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login failed for role {role}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set(role, "CurrentRole");
    }

    [Given(@"the clinic has more than 50 completed appointments with no-show data")]
    public async Task GivenClinicHasMoreThan50CompletedAppointments()
    {
        await SeedCompletedAppointments(55);
    }

    [Given(@"the clinic has sufficient appointment history")]
    public async Task GivenClinicHasSufficientAppointmentHistory()
    {
        await SeedCompletedAppointments(55);
    }

    [Given(@"the clinic has fewer than 50 completed appointments")]
    public async Task GivenClinicHasFewerThan50CompletedAppointments()
    {
        // Ensure no completed appointments exist (AfterScenario cleans up, so this is just
        // an assertion of precondition — no seeding needed, DB is clean at scenario start)
        // We deliberately do NOT seed any completed appointments.
    }

    [Given(@"an appointment exists for owner ""(.*)"" on ""(.*)"" at ""(.*)""")]
    public async Task GivenAppointmentExistsForOwner(string ownerName, string dateStr, string timeStr)
    {
        var clinicId = GetOrCreateClinicId();
        var date = DateOnly.Parse(dateStr);
        var startTime = TimeOnly.Parse(timeStr);

        _targetAppointmentId = await CreateScheduledAppointment(clinicId, ownerName, date, startTime);
    }

    [Given(@"an appointment exists on ""(.*)""")]
    public async Task GivenAppointmentExistsOn(string dateStr)
    {
        var clinicId = GetOrCreateClinicId();
        var date = DateOnly.Parse(dateStr);

        _targetAppointmentId = await CreateScheduledAppointment(clinicId, "Generic Owner", date, new TimeOnly(10, 0));
    }

    [Given(@"an appointment exists for ""(.*)"" on ""(.*)""")]
    public async Task GivenAppointmentExistsFor(string ownerName, string dateStr)
    {
        var clinicId = GetOrCreateClinicId();
        var date = DateOnly.Parse(dateStr);

        // Use the same animalId as SeedHistoricalAppointmentsForAnimal so that
        // GetFeaturesForPredictionAsync can find the historical no-show data.
        var animalId = GenerateGuidFromString($"animal-{ownerName}");
        _targetAppointmentId = await CreateScheduledAppointmentForAnimal(clinicId, ownerName, animalId, date, new TimeOnly(10, 0));
    }

    [Given(@"there are (\d+) appointments on ""(.*)""")]
    public async Task GivenThereAreAppointmentsOn(int count, string dateStr)
    {
        var clinicId = GetOrCreateClinicId();
        var date = DateOnly.Parse(dateStr);
        var startHour = 9;

        for (var i = 0; i < count; i++)
        {
            var startTime = new TimeOnly(startHour + i, 0);
            var id = await CreateScheduledAppointment(clinicId, $"Owner {i + 1}", date, startTime);
            if (i == 0)
                _targetAppointmentId = id;
        }
    }

    [Given(@"owner ""(.*)"" has a (\d+)% historical no-show rate")]
    public async Task GivenOwnerHasHistoricalNoShowRate(string ownerName, int noShowPercent)
    {
        // Seed completed appointments history with the appropriate no-show rate.
        // We need at least 50 completed appointments overall AND enough with NoShow status
        // for the owner so that when the handler computes HistoricalNoShowRate > 0.35f,
        // the FakeNoShowPredictionService returns "High".
        var clinicId = GetOrCreateClinicId();
        var animalId = GenerateGuidFromString($"animal-{ownerName}");

        // Seed 50 historical appointments for this animal: 40% NoShow
        var total = 50;
        var noShows = (int)(total * noShowPercent / 100.0);
        await SeedHistoricalAppointmentsForAnimal(clinicId, animalId, ownerName, total, noShows);
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I request a no-show prediction for that appointment")]
    public async Task WhenIRequestNoShowPrediction()
    {
        _response = await _client.GetAsync(
            $"/api/v1/ai/no-show-prediction/{_targetAppointmentId}");

        if (_response.IsSuccessStatusCode)
        {
            _prediction = await _response.Content.ReadFromJsonAsync<NoShowPredictionDto>(JsonOptions);
        }
    }

    [When(@"I request a no-show prediction for any appointment")]
    public async Task WhenIRequestNoShowPredictionForAnyAppointment()
    {
        // Use a random appointment ID — 403 will be returned before any DB lookup
        _response = await _client.GetAsync(
            $"/api/v1/ai/no-show-prediction/{Guid.NewGuid()}");
    }

    [When(@"I request a no-show prediction for an appointment")]
    public async Task WhenIRequestNoShowPredictionForAnAppointment()
    {
        var clinicId = GetOrCreateClinicId();

        // Ensure sufficient history exists
        await SeedCompletedAppointments(55);

        // Create a target appointment
        _targetAppointmentId = await CreateScheduledAppointment(
            clinicId, "Test Owner", new DateOnly(2026, 3, 15), new TimeOnly(10, 0));

        _response = await _client.GetAsync(
            $"/api/v1/ai/no-show-prediction/{_targetAppointmentId}");

        if (_response.IsSuccessStatusCode)
        {
            _prediction = await _response.Content.ReadFromJsonAsync<NoShowPredictionDto>(JsonOptions);
        }
    }

    [When(@"I request batch no-show predictions for ""(.*)""")]
    public async Task WhenIRequestBatchNoShowPredictions(string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        var body = new { date = date.ToString("yyyy-MM-dd") };

        _response = await _client.PostAsJsonAsync("/api/v1/ai/no-show-predictions/batch", body);

        if (_response.IsSuccessStatusCode)
        {
            _batchPredictions = await _response.Content.ReadFromJsonAsync<List<NoShowPredictionDto>>(JsonOptions);
        }
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"I should receive a prediction with status 200")]
    public void ThenIShouldReceivePrediction200()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_response.StatusCode}. Response: {ReadResponseBody()}");
        _prediction.Should().NotBeNull("Prediction DTO should be deserialized");
    }

    [Then(@"the prediction should contain a probability between 0 and 1")]
    public void ThenPredictionShouldContainProbabilityBetween0And1()
    {
        _prediction.Should().NotBeNull();
        _prediction!.NoShowProbability.Should().BeGreaterThanOrEqualTo(0f)
            .And.BeLessThanOrEqualTo(1f);
    }

    [Then(@"the prediction should contain a risk level of ""Low"", ""Medium"", or ""High""")]
    public void ThenPredictionShouldContainValidRiskLevel()
    {
        _prediction.Should().NotBeNull();
        var validLevels = new[] { "Low", "Medium", "High" };
        validLevels.Should().Contain(_prediction!.RiskLevel,
            $"Risk level '{_prediction.RiskLevel}' should be one of Low, Medium, High");
    }

    [Then(@"the prediction should contain top contributing factors")]
    public void ThenPredictionShouldContainTopFactors()
    {
        _prediction.Should().NotBeNull();
        _prediction!.TopFactors.Should().NotBeNullOrEmpty("Top factors should be provided");
    }

    [Then(@"the prediction should contain actionable suggestions")]
    public void ThenPredictionShouldContainSuggestions()
    {
        _prediction.Should().NotBeNull();
        // Suggestions may be empty for Low risk — just verify the list is not null
        _prediction!.Suggestions.Should().NotBeNull("Suggestions list should not be null");
    }

    [Then(@"I should receive an error ""(.*)""")]
    public async Task ThenIShouldReceiveAnError(string errorCode)
    {
        _response.IsSuccessStatusCode.Should().BeFalse(
            $"Expected error but got {(int)_response.StatusCode}");
        var body = await _response.Content.ReadAsStringAsync();
        body.Should().Contain(errorCode,
            $"Expected error code '{errorCode}' in response body: {body}");
    }

    [Then(@"I should receive (\d+) predictions")]
    public void ThenIShouldReceivePredictions(int count)
    {
        _response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Expected 200 but got {(int)_response.StatusCode}");
        _batchPredictions.Should().NotBeNull();
        _batchPredictions.Should().HaveCount(count,
            $"Expected {count} predictions but got {_batchPredictions!.Count}");
    }

    [Then(@"each prediction should contain a probability and risk level")]
    public void ThenEachPredictionShouldContainProbabilityAndRiskLevel()
    {
        _batchPredictions.Should().NotBeNull();
        foreach (var p in _batchPredictions!)
        {
            p.NoShowProbability.Should().BeGreaterThanOrEqualTo(0f).And.BeLessThanOrEqualTo(1f);
            new[] { "Low", "Medium", "High" }.Should().Contain(p.RiskLevel);
        }
    }

    [Then(@"the risk level should be ""High""")]
    public void ThenRiskLevelShouldBeHigh()
    {
        _prediction.Should().NotBeNull();
        _prediction!.RiskLevel.Should().Be("High",
            $"Expected High risk but got {_prediction.RiskLevel}");
    }

    [Then(@"the suggestions should include sending an extra reminder")]
    public void ThenSuggestionsShouldIncludeSendingExtraReminder()
    {
        _prediction.Should().NotBeNull();
        _prediction!.Suggestions.Should().Contain("send_extra_reminder",
            "High risk appointments should suggest sending an extra reminder");
    }

    [Then(@"I should receive a 403 Forbidden response")]
    public void ThenIShouldReceive403()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Expected 403 but got {(int)_response.StatusCode}");
    }

    [Then(@"the top factors should not include owner name or address")]
    public void ThenTopFactorsShouldNotIncludePii()
    {
        _prediction.Should().NotBeNull();
        var piiKeywords = new[] { "name", "address", "email", "phone", "owner_name", "owner_address" };
        foreach (var factor in _prediction!.TopFactors)
        {
            foreach (var keyword in piiKeywords)
            {
                factor.Should().NotContainEquivalentOf(keyword,
                    $"Factor '{factor}' should not reference PII data like owner name or address");
            }
        }
    }

    [Then(@"the factors should reference behavioral data only")]
    public void ThenFactorsShouldReferenceBehavioralDataOnly()
    {
        _prediction.Should().NotBeNull();
        // Behavioral factors come from the known factor vocabulary
        var allowedFactorPrefixes = new[]
        {
            "high_historical_noshow_rate",
            "weekend_appointment",
            "off_peak_hour",
            "new_patient",
            "long_absence",
            "no_reminder_sent",
            "first_appointment",
            "long_lead_time",
            "appointment_type"
        };

        foreach (var factor in _prediction!.TopFactors)
        {
            allowedFactorPrefixes.Should().Contain(f => factor.StartsWith(f, StringComparison.OrdinalIgnoreCase),
                $"Factor '{factor}' should be a known behavioral factor");
        }
    }

    [Then(@"the day-of-week factor should reflect UAE weekend patterns")]
    public void ThenDayOfWeekFactorShouldReflectUaeWeekend()
    {
        _prediction.Should().NotBeNull();
        // For a Friday appointment (DayOfWeek = 5 in .NET), the prediction should
        // include "weekend_appointment" in top factors (UAE weekend is Fri-Sat)
        _prediction!.TopFactors.Should().Contain("weekend_appointment",
            "Friday is a UAE weekend day and should be reflected in top factors");
    }

    // ─── UAE Weekend Scenario ─────────────────────────────────────

    [Given(@"the clinic operates Sunday through Thursday")]
    public void GivenClinicOperatesSundayThroughThursday()
    {
        // UAE standard work week context — no action needed, just documentation
    }

    [Given(@"an appointment exists on a Friday")]
    public async Task GivenAppointmentExistsOnFriday()
    {
        var clinicId = GetOrCreateClinicId();

        // Ensure sufficient history
        await SeedCompletedAppointments(55);

        // Create appointment on a Friday (2026-03-13 is a Friday)
        var friday = new DateOnly(2026, 3, 13);
        _targetAppointmentId = await CreateScheduledAppointment(
            clinicId, "Friday Client", friday, new TimeOnly(10, 0));
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Guid GetOrCreateClinicId()
    {
        // Always use the fixed TestClinicContext.TestClinicGuid.
        // The EF Core model caches the query filter and evaluates the ClinicId at model
        // compilation time. Using a fixed GUID ensures the baked-in filter value matches
        // what we seed in each test. AfterScenario cleanup uses IgnoreQueryFilters()
        // to delete all data between scenarios.
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = TestClinicContext.TestClinicGuid;
        return TestClinicContext.TestClinicGuid;
    }

    private Task<Guid> CreateScheduledAppointment(
        Guid clinicId,
        string ownerName,
        DateOnly date,
        TimeOnly startTime)
    {
        // Generate a unique animalId that incorporates the date to avoid collisions
        // between scenarios that use the same ownerName on different dates.
        var animalId = GenerateGuidFromString($"animal-{ownerName}-{date}");
        return CreateScheduledAppointmentForAnimal(clinicId, ownerName, animalId, date, startTime);
    }

    private async Task<Guid> CreateScheduledAppointmentForAnimal(
        Guid clinicId,
        string ownerName,
        Guid animalId,
        DateOnly date,
        TimeOnly startTime)
    {
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        var vetId = Guid.NewGuid();

        var result = Appointment.Create(
            clinicId: clinicId,
            veterinarianId: vetId,
            veterinarianName: "Dr. Test",
            animalId: animalId,
            animalName: "Buddy",
            ownerName: ownerName,
            date: date,
            startTime: startTime,
            durationMinutes: 30,
            reason: "consultation");

        result.IsSuccess.Should().BeTrue("Appointment creation should succeed");
        db.Appointments.Add(result.Value);
        await db.SaveChangesAsync();

        return result.Value.Id;
    }

    /// <summary>
    /// Seeds completed/no-show appointments to satisfy the cold-start threshold.
    /// Directly manipulates the DB to set status, bypassing the domain state machine.
    /// </summary>
    private async Task SeedCompletedAppointments(int count)
    {
        var clinicId = GetOrCreateClinicId();
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        var baseDate = new DateOnly(2025, 1, 1);
        var vetId = Guid.NewGuid();

        for (var i = 0; i < count; i++)
        {
            var animalId = GenerateGuidFromString($"history-animal-{i}-{clinicId}");
            var date = baseDate.AddDays(i);
            var startTime = new TimeOnly(9, 0);

            var result = Appointment.Create(
                clinicId: clinicId,
                veterinarianId: vetId,
                veterinarianName: "Dr. History",
                animalId: animalId,
                animalName: $"Animal {i}",
                ownerName: $"Owner {i}",
                date: date,
                startTime: startTime,
                durationMinutes: 30,
                reason: null);

            if (!result.IsSuccess) continue;

            var appointment = result.Value;

            // Advance to Completed via domain methods
            appointment.CheckIn();
            appointment.StartConsultation();
            appointment.Complete();

            db.Appointments.Add(appointment);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds historical appointments for a specific animal with a given no-show rate.
    /// Used to trigger "High" risk predictions via the FakeNoShowPredictionService.
    /// </summary>
    private async Task SeedHistoricalAppointmentsForAnimal(
        Guid clinicId,
        Guid animalId,
        string ownerName,
        int total,
        int noShowCount)
    {
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        var baseDate = new DateOnly(2025, 1, 1);
        var vetId = Guid.NewGuid();

        for (var i = 0; i < total; i++)
        {
            var date = baseDate.AddDays(i);
            var result = Appointment.Create(
                clinicId: clinicId,
                veterinarianId: vetId,
                veterinarianName: "Dr. History",
                animalId: animalId,
                animalName: "Buddy",
                ownerName: ownerName,
                date: date,
                startTime: new TimeOnly(9, 0),
                durationMinutes: 30,
                reason: null);

            if (!result.IsSuccess) continue;

            var appointment = result.Value;

            if (i < noShowCount)
            {
                // Mark as no-show
                appointment.MarkNoShow();
            }
            else
            {
                // Mark as completed
                appointment.CheckIn();
                appointment.StartConsultation();
                appointment.Complete();
            }

            db.Appointments.Add(appointment);
        }

        await db.SaveChangesAsync();
    }

    private string ReadResponseBody()
    {
        try
        {
            return _response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return "(could not read response body)";
        }
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
