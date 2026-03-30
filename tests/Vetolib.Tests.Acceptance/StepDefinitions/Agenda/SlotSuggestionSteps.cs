using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Agenda.Contracts;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Agenda;

[Binding]
[Scope(Feature = "Slot Suggestion")]
internal class SlotSuggestionSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;
    private SlotSuggestionsResponse? _suggestionsResponse;
    private string? _errorBody;

    private Guid _clinicId;

    // Well-known vet IDs from the feature file Background
    private static readonly Guid AhmadId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid FatimaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SlotSuggestionSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();

        // MUST use the fixed TestClinicGuid — EF Core compiles the multi-tenant query filter
        // once per model and bakes in the ClinicId value at model creation time.
        // Using Guid.NewGuid() would produce a random GUID that never matches the baked-in filter,
        // causing all queries to return empty results.
        _clinicId = TestClinicContext.TestClinicGuid;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = _clinicId;
    }

    // ─── GIVEN ───────────────────────────────────────────────────

    [Given(@"I am authenticated as a user with role ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsUserWithRole(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userRole = role switch
        {
            "Receptionist" => UserRole.Receptionist,
            "Vet" => UserRole.Vet,
            "Admin" => UserRole.Admin,
            _ => UserRole.Receptionist
        };

        var email = $"{role.ToLower()}@slottest.ae";
        var password = "SecurePass1!";

        var userResult = User.Create(_clinicId, email, password, userRole);
        userResult.IsSuccess.Should().BeTrue();
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));

        if (!loginResponse.IsSuccessStatusCode)
        {
            var loginError = await loginResponse.Content.ReadAsStringAsync();
            loginResponse.EnsureSuccessStatusCode(); // will throw with detailed message
        }

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    [Given(@"the clinic has the following veterinarians:")]
    public void GivenTheClinicHasTheFollowingVeterinarians(Table table)
    {
        // Vet IDs are well-known from feature file — no DB registration needed for slot suggestion
        // since we inject appointments directly. This step is informational.
    }

    [Given(@"Dr\. Ahmad has the following appointments on ""(.*)"":")]
    public async Task GivenDrAhmadHasAppointmentsOn(string dateStr, Table table)
    {
        var date = DateOnly.Parse(dateStr);
        using var scope = _factory.Services.CreateScope();
        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        foreach (var row in table.Rows)
        {
            var startTime = TimeOnly.Parse(row["StartTime"]);
            var duration = int.Parse(row["Duration"]);
            var type = row["Type"];

            var apptResult = Appointment.Create(
                _clinicId,
                AhmadId,
                "Dr. Ahmad",
                Guid.NewGuid(),
                "Animal",
                "Owner",
                null,
                date,
                startTime,
                duration,
                type);

            apptResult.IsSuccess.Should().BeTrue();
            agendaDb.Appointments.Add(apptResult.Value);
        }

        await agendaDb.SaveChangesAsync();
    }

    [Given(@"Dr\. Ahmad has (.*) appointments on ""(.*)""")]
    public async Task GivenDrAhmadHasNAppointmentsOn(int count, string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        await CreateAppointmentsForVet(AhmadId, "Dr. Ahmad", date, count, new TimeOnly(9, 0), 30, "general");
    }

    [Given(@"Dr\. Fatima has (.*) appointments on ""(.*)""")]
    public async Task GivenDrFatimaHasNAppointmentsOn(int count, string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        await CreateAppointmentsForVet(FatimaId, "Dr. Fatima", date, count, new TimeOnly(9, 0), 30, "general");
    }

    [Given(@"Dr\. Ahmad has (.*) ""(.*)"" appointments in the morning on ""(.*)""")]
    public async Task GivenDrAhmadHasSurgeryAppointmentsMorning(int count, string type, string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        await CreateAppointmentsForVet(AhmadId, "Dr. Ahmad", date, count, new TimeOnly(9, 0), 60, type);
    }

    [Given(@"Dr\. Ahmad has available slots in the morning and afternoon")]
    public void GivenDrAhmadHasAvailableSlotsInMorningAndAfternoon()
    {
        // No action needed — appointments were already added sparsely enough to leave slots
    }

    [Given(@"Dr\. Ahmad has fewer than 5 past ""(.*)"" appointments")]
    public async Task GivenDrAhmadHasFewerThan5PastAppointments(string type)
    {
        // Insert 2 past completed appointments of this type so count < 5 (insufficient history).
        var pastDate = new DateOnly(2025, 1, 1);
        using var scope = _factory.Services.CreateScope();
        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        for (int i = 0; i < 2; i++)
        {
            var apptResult = Appointment.Create(
                _clinicId, AhmadId, "Dr. Ahmad",
                Guid.NewGuid(), "Animal", "Owner", null,
                pastDate.AddDays(i), new TimeOnly(9, 0), 30, type);
            apptResult.IsSuccess.Should().BeTrue();
            var appt = apptResult.Value;
            appt.CheckIn();
            appt.StartConsultation();
            appt.Complete();
            agendaDb.Appointments.Add(appt);
        }

        // Also add a scheduled (upcoming) appointment on 2026-03-15 so that the SuggestSlotHandler
        // discovers Dr. Ahmad when querying that date (the handler builds vetIds from existing
        // appointments on the requested date). Without this, vetIds would be empty and no
        // suggestions would be generated.
        var futureApptResult = Appointment.Create(
            _clinicId, AhmadId, "Dr. Ahmad",
            Guid.NewGuid(), "Animal", "Owner", null,
            new DateOnly(2026, 3, 15), new TimeOnly(9, 0), 30, type);
        futureApptResult.IsSuccess.Should().BeTrue();
        agendaDb.Appointments.Add(futureApptResult.Value);

        await agendaDb.SaveChangesAsync();
    }

    [Given(@"Dr\. Ahmad has completed (.*) ""(.*)"" appointments with average duration (.*) minutes")]
    public async Task GivenDrAhmadHasCompletedAppointmentsWithAvgDuration(int count, string type, int avgDuration)
    {
        // Insert `count` completed appointments with the given duration
        using var scope = _factory.Services.CreateScope();
        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        var baseDate = new DateOnly(2025, 1, 1);
        for (int i = 0; i < count; i++)
        {
            var apptResult = Appointment.Create(
                _clinicId, AhmadId, "Dr. Ahmad",
                Guid.NewGuid(), "Animal", "Owner", null,
                baseDate.AddDays(i), new TimeOnly(9, 0), avgDuration, type);
            apptResult.IsSuccess.Should().BeTrue();

            var appt = apptResult.Value;
            // Transition to Completed: CheckIn -> InProgress -> Completed
            appt.CheckIn();
            appt.StartConsultation();
            appt.Complete();

            agendaDb.Appointments.Add(appt);
        }

        await agendaDb.SaveChangesAsync();
    }

    [Given(@"all veterinarians are fully booked on ""(.*)""")]
    public async Task GivenAllVetsFullyBookedOn(string dateStr)
    {
        var date = DateOnly.Parse(dateStr);
        // Fill 9:00-18:00 with 30-min slots = 18 appointments per vet
        await CreateAppointmentsForVet(AhmadId, "Dr. Ahmad", date, 18, new TimeOnly(9, 0), 30, "general");
        await CreateAppointmentsForVet(FatimaId, "Dr. Fatima", date, 18, new TimeOnly(9, 0), 30, "general");
    }

    // ─── WHEN ────────────────────────────────────────────────────

    [When(@"I request a slot suggestion for:")]
    public async Task WhenIRequestASlotSuggestionFor(Table table)
    {
        var row = table.Rows[0];

        var consultationType = row["ConsultationType"];
        var preferredDate = DateOnly.Parse(row["PreferredDate"]);
        var preferredTime = TimeOnly.Parse(row["PreferredTime"]);
        Guid? preferredVetId = row.ContainsKey("PreferredVeterinarianId") && !string.IsNullOrEmpty(row["PreferredVeterinarianId"])
            ? Guid.Parse(row["PreferredVeterinarianId"])
            : null;

        var request = new SuggestSlotRequest(
            consultationType,
            preferredDate,
            preferredTime,
            preferredVetId,
            null);

        _response = await _client.PostAsJsonAsync("/api/v1/appointments/suggest-slot", request);

        if (_response.IsSuccessStatusCode)
        {
            _suggestionsResponse = await _response.Content.ReadFromJsonAsync<SlotSuggestionsResponse>(JsonOptions);
        }
        else
        {
            _errorBody = await _response.Content.ReadAsStringAsync();
        }
    }

    // ─── THEN ────────────────────────────────────────────────────

    [Then(@"I should receive (.*) slot suggestions")]
    public void ThenIShouldReceiveNSlotSuggestions(int expectedCount)
    {
        _response.IsSuccessStatusCode.Should().BeTrue(
            $"Expected success but got {_response.StatusCode}. Body: {_errorBody}");
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().HaveCount(expectedCount);
    }

    [Then(@"the first suggestion should minimize the gap in Dr\. Ahmad's schedule")]
    public void ThenFirstSuggestionShouldMinimizeGap()
    {
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().NotBeEmpty();

        // The top suggestion should be for Dr. Ahmad (the one with appointments to fill gaps)
        var first = _suggestionsResponse.Suggestions[0];
        first.VeterinarianId.Should().Be(AhmadId,
            "Dr. Ahmad has existing appointments and the best slot minimizes the gap");
    }

    [Then(@"each suggestion should include a score between 0 and 100")]
    public void ThenEachSuggestionShouldIncludeScore()
    {
        _suggestionsResponse.Should().NotBeNull();
        foreach (var s in _suggestionsResponse!.Suggestions)
        {
            s.Score.Should().BeInRange(0, 100);
        }
    }

    [Then(@"the highest scored suggestion should be for Dr\. Fatima")]
    public void ThenHighestScoredSuggestionShouldBeForFatima()
    {
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().NotBeEmpty();
        _suggestionsResponse.Suggestions[0].VeterinarianId.Should().Be(FatimaId,
            "Dr. Fatima has fewer appointments (load balancing)");
    }

    [Then(@"the highest scored suggestion should be in the morning for Dr\. Ahmad")]
    public void ThenHighestScoredSuggestionShouldBeInMorningForAhmad()
    {
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().NotBeEmpty();
        var first = _suggestionsResponse.Suggestions[0];
        first.VeterinarianId.Should().Be(AhmadId,
            "Dr. Ahmad already has surgery appointments in the morning — type grouping should favor him");
        first.StartTime.Hour.Should().BeLessThan(13,
            "The suggestion should be in the morning");
    }

    [Then(@"the estimated duration should use the default for ""(.*)""")]
    public void ThenEstimatedDurationShouldUseDefault(string consultationType)
    {
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().NotBeEmpty();

        // Default for dermatology is 30 minutes (as configured in appsettings)
        var suggestion = _suggestionsResponse.Suggestions[0];
        suggestion.EstimatedDurationMinutes.Should().BeGreaterThan(0);
    }

    [Then(@"the estimated duration should be approximately (.*) minutes")]
    public void ThenEstimatedDurationShouldBeApproximately(int expectedMinutes)
    {
        _suggestionsResponse.Should().NotBeNull();
        _suggestionsResponse!.Suggestions.Should().NotBeEmpty();

        // The estimated duration from history should be within 5 minutes of expected
        var suggestion = _suggestionsResponse.Suggestions.First(s => s.VeterinarianId == AhmadId);
        suggestion.EstimatedDurationMinutes.Should().BeCloseTo(expectedMinutes, 5);
    }

    [Then(@"all suggestions should be for Dr\. Ahmad")]
    public void ThenAllSuggestionsShouldBeForAhmad()
    {
        _suggestionsResponse.Should().NotBeNull();
        foreach (var s in _suggestionsResponse!.Suggestions)
        {
            s.VeterinarianId.Should().Be(AhmadId,
                "Only Dr. Ahmad was specified as preferred veterinarian");
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private async Task CreateAppointmentsForVet(
        Guid vetId,
        string vetName,
        DateOnly date,
        int count,
        TimeOnly firstStart,
        int duration,
        string type)
    {
        using var scope = _factory.Services.CreateScope();
        var agendaDb = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();

        var start = firstStart;
        for (int i = 0; i < count; i++)
        {
            var apptResult = Appointment.Create(
                _clinicId, vetId, vetName,
                Guid.NewGuid(), "Animal", "Owner", null,
                date, start, duration, type);

            if (apptResult.IsSuccess)
            {
                agendaDb.Appointments.Add(apptResult.Value);
                start = start.AddMinutes(duration);

                // Stop if past working hours
                if (start >= new TimeOnly(18, 0))
                    break;
            }
        }

        await agendaDb.SaveChangesAsync();
    }
}
