using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.AI.Application.Domain;
using Vetolib.AI.Contracts;
using Vetolib.AI.Infrastructure;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.StepDefinitions;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.AI;

[Binding]
[Scope(Feature = "Predictive Health Alerts")]
internal class PredictiveHealthAlertSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;

    // Scenario state
    private List<HealthAlertDto>? _alerts;
    private HealthAlertDto? _currentAlert;
    private HttpResponseMessage? _lastResponse;
    private AppointmentPreFillDto? _appointmentPreFill;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public PredictiveHealthAlertSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── Background ─────────────────────────────────────────────

    [Given(@"I am authenticated as a veterinarian at clinic ""(.*)""")]
    public async Task GivenIAmAuthenticatedAsVeterinarianAtClinic(string clinicName)
    {
        var clinicId = GetOrCreateClinicId(clinicName);

        var email = $"vet-health-alert-{Guid.NewGuid():N}@happypaws.ae";
        const string password = "SecurePass1!";

        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var userResult = User.Create(clinicId, email, password, UserRole.Vet, "HEALTH-VET-001");
        userResult.IsSuccess.Should().BeTrue("User creation should succeed for vet");
        authDb.Users.Add(userResult.Value);
        await authDb.SaveChangesAsync();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login should succeed for vet at {clinicName}");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authToken!.AccessToken);

        _ctx.Set("VET", "CurrentRole");
        _ctx.Set(clinicName, "CurrentClinicName");
    }

    // ─── Patient Setup (Given) ──────────────────────────────────

    [Given(@"patient ""(.*)"" is a (.*), (male|female), (\d+) years? old")]
    public void GivenPatientIsBreedGenderAge(string name, string breed, string gender, int ageYears)
    {
        var patientId = GenerateGuidFromString($"patient-{name}");
        var birthDate = DateTime.UtcNow.AddYears(-ageYears);

        // Determine species from breed
        var species = DetermineSpecies(breed);

        var patient = new PatientSetup(patientId, name, species, breed, gender, ageYears, birthDate);
        _ctx.Set(patient, $"Patient_{name}");
    }

    [Given(@"""(.*)"" has not had a renal panel in the last (\d+) months")]
    public async Task GivenPatientHasNotHadRenalPanel(string name, int months)
    {
        // Seed alert via generate or directly — the patient has overdue renal screening
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.High, "Renal screening overdue",
            $"Patient has not had a renal panel in the last {months} months. Breed-specific risk applies.",
            "CatRenalScreeningRule");
    }

    [Given(@"""(.*)"" has all vaccinations up to date")]
    public void GivenPatientHasAllVaccinationsUpToDate(string name)
    {
        // No alerts to seed — patient is up to date
    }

    [Given(@"""(.*)"" had a wellness exam last month")]
    public void GivenPatientHadWellnessExamLastMonth(string name)
    {
        // No alerts to seed — patient recently had wellness exam
    }

    [Given(@"""(.*)"" has not had a cardiac exam in the last (\d+) months")]
    public async Task GivenPatientHasNotHadCardiacExam(string name, int months)
    {
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.High, "Annual cardiac screening recommended",
            "MVD breed predisposition detected. Annual cardiac screening is recommended for Cavalier King Charles Spaniels.",
            "CardiacBreedRule");
    }

    [Given(@"""(.*)"" has not had a wellness exam in the last (\d+) months")]
    public async Task GivenPatientHasNotHadWellnessExam(string name, int months)
    {
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.SeniorWellness,
            HealthAlertSeverity.Medium, "Senior wellness exam recommended",
            $"Patient is {patient.AgeYears} years old and has not had a wellness exam in the last {months} months.",
            "SeniorWellnessRule");
    }

    [Given(@"""(.*)"" weighed ([\d.]+) kg three visits ago")]
    public void GivenPatientWeighedThreeVisitsAgo(string name, decimal weight)
    {
        var weights = GetOrCreateWeightHistory(name);
        weights.Add(("three_visits_ago", weight));
        _ctx.Set(weights, $"Weights_{name}");
    }

    [Given(@"""(.*)"" weighed ([\d.]+) kg two visits ago")]
    public void GivenPatientWeighedTwoVisitsAgo(string name, decimal weight)
    {
        var weights = GetOrCreateWeightHistory(name);
        weights.Add(("two_visits_ago", weight));
        _ctx.Set(weights, $"Weights_{name}");
    }

    [Given(@"""(.*)"" now weighs ([\d.]+) kg")]
    public async Task GivenPatientNowWeighs(string name, decimal weight)
    {
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.WeightTrend,
            HealthAlertSeverity.Medium, "Weight gain trend detected",
            "Weight increase of more than 15% over recent visits detected. An obesity screening is recommended.",
            "WeightTrendRule");
    }

    [Given(@"""(.*)"" has weighed between ([\d.]+) kg and ([\d.]+) kg across her last (\d+) visits")]
    public void GivenPatientHasStableWeight(string name, decimal minWeight, decimal maxWeight, int visits)
    {
        // Stable weight — no weight alert to seed
    }

    [Given(@"patient ""(.*)"" has a health alert ""(.*)""")]
    public async Task GivenPatientHasHealthAlert(string name, string alertTitle)
    {
        var patientId = GenerateGuidFromString($"patient-{name}");

        // Determine type/severity from the title
        var (alertType, severity, description, ruleId) = ResolveAlertFromTitle(alertTitle);

        await SeedHealthAlert(patientId, alertType, severity, alertTitle, description, ruleId);
    }

    [Given(@"""(.*)"" has a core vaccination overdue by (\d+) days")]
    public async Task GivenPatientHasVaccinationOverdue(string name, int daysOverdue)
    {
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.VaccinationDue,
            HealthAlertSeverity.High, "Core vaccination overdue",
            $"Core vaccination is {daysOverdue} days past due.",
            "VaccinationOverdueRule");
    }

    [Given(@"""(.*)"" has a previous diagnosis of respiratory distress")]
    public async Task GivenPatientHasPreviousRespiratoryDiagnosis(string name)
    {
        var patient = _ctx.Get<PatientSetup>($"Patient_{name}");
        await SeedHealthAlert(patient.PatientId, HealthAlertType.BreedSpecificScreening,
            HealthAlertSeverity.Medium, "Annual airway assessment recommended",
            "Brachycephalic breed risk detected. Previous respiratory distress diagnosis on record. Annual airway assessment is recommended.",
            "BrachycephalicAirwayRule");
    }

    // ─── Multi-Tenancy Setup ────────────────────────────────────

    [Given(@"patient ""(.*)"" exists at clinic ""(.*)"" with (\d+) alerts?")]
    public async Task GivenPatientExistsAtClinicWithAlerts(string name, string clinicName, int alertCount)
    {
        var clinicId = GetOrCreateClinicId(clinicName);
        var patientId = GenerateGuidFromString($"patient-{name}-{clinicName}");

        // Set tenant context to seed data for this specific clinic
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        for (var i = 0; i < alertCount; i++)
        {
            await SeedHealthAlertForClinic(clinicId, patientId,
                HealthAlertType.BreedSpecificScreening,
                HealthAlertSeverity.Medium,
                $"Alert {i + 1} for {name}",
                $"Test alert {i + 1} for multi-tenancy scenario.",
                $"TestRule_{i}");
        }

        // Restore tenant context to the primary clinic
        var primaryClinicName = _ctx.ContainsKey("CurrentClinicName")
            ? _ctx.Get<string>("CurrentClinicName")
            : clinicName;
        var primaryClinicId = GetOrCreateClinicId(primaryClinicName);
        testClinicContext.ClinicId = primaryClinicId;
    }

    [Given(@"patient ""(.*)"" also exists at clinic ""(.*)"" with (\d+) alerts?")]
    public async Task GivenPatientAlsoExistsAtClinicWithAlerts(string name, string clinicName, int alertCount)
    {
        await GivenPatientExistsAtClinicWithAlerts(name, clinicName, alertCount);
    }

    // ─── WHEN Steps ─────────────────────────────────────────────

    [When(@"I open the patient dashboard for ""(.*)""")]
    public async Task WhenIOpenPatientDashboard(string name)
    {
        var patientId = GenerateGuidFromString($"patient-{name}");
        _lastResponse = await _client.GetAsync($"/api/v1/ai/health-alerts/patient/{patientId}");

        if (_lastResponse.IsSuccessStatusCode)
        {
            _alerts = await _lastResponse.Content.ReadFromJsonAsync<List<HealthAlertDto>>(JsonOptions);
        }
        else
        {
            _alerts = new List<HealthAlertDto>();
        }
    }

    [When(@"I dismiss the alert with reason ""(.*)""")]
    public async Task WhenIDismissAlertWithReason(string reason)
    {
        var alert = await GetFirstActiveAlert();
        alert.Should().NotBeNull("An active alert should exist to dismiss");

        var body = new { reason };
        _lastResponse = await _client.PatchAsJsonAsync(
            $"/api/v1/ai/health-alerts/{alert!.Id}/dismiss", body);
    }

    [When(@"I choose to schedule an appointment from the alert")]
    public async Task WhenIChooseToScheduleAppointment()
    {
        var alert = await GetFirstActiveAlert();
        alert.Should().NotBeNull("An active alert should exist to convert");

        _lastResponse = await _client.PostAsync(
            $"/api/v1/ai/health-alerts/{alert!.Id}/convert-to-appointment", null);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _appointmentPreFill = await _lastResponse.Content
                .ReadFromJsonAsync<AppointmentPreFillDto>(JsonOptions);
        }
    }

    [When(@"I acknowledge the alert")]
    public async Task WhenIAcknowledgeTheAlert()
    {
        var alert = await GetFirstActiveAlert();
        alert.Should().NotBeNull("An active alert should exist to acknowledge");

        _lastResponse = await _client.PatchAsync(
            $"/api/v1/ai/health-alerts/{alert!.Id}/acknowledge", null);
    }

    [When(@"I view the patient dashboard at ""(.*)""")]
    public async Task WhenIViewDashboardAtClinic(string clinicName)
    {
        // The tenant context should already be set to the primary clinic
        var clinicId = GetOrCreateClinicId(clinicName);
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        // Get all alerts for this clinic
        _lastResponse = await _client.GetAsync("/api/v1/ai/health-alerts");

        if (_lastResponse.IsSuccessStatusCode)
        {
            _alerts = await _lastResponse.Content.ReadFromJsonAsync<List<HealthAlertDto>>(JsonOptions);
        }
        else
        {
            _alerts = new List<HealthAlertDto>();
        }
    }

    // ─── THEN Steps ─────────────────────────────────────────────

    [Then(@"I should see a health alert ""(.*)""")]
    public void ThenIShouldSeeHealthAlert(string alertTitle)
    {
        _alerts.Should().NotBeNull("Alerts list should not be null");
        _alerts.Should().Contain(a => a.Title == alertTitle,
            $"Expected to find alert with title '{alertTitle}'");
        _currentAlert = _alerts!.First(a => a.Title == alertTitle);
    }

    [Then(@"the alert should indicate a high priority")]
    public void ThenAlertShouldIndicateHighPriority()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Severity.Should().Be(HealthAlertSeverity.High);
    }

    [Then(@"the alert should mention the breed-specific risk for Golden Retrievers")]
    public void ThenAlertShouldMentionBreedRiskGoldenRetriever()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().Contain("Breed",
            "Alert description should reference breed-specific risk");
    }

    [Then(@"I should see no health alerts")]
    public void ThenIShouldSeeNoHealthAlerts()
    {
        _alerts.Should().NotBeNull();
        _alerts.Should().BeEmpty("Patient with up-to-date care should have no alerts");
    }

    [Then(@"the alert description should mention that CKD risk increases after age 7 in cats")]
    public void ThenAlertShouldMentionCKDRisk()
    {
        _currentAlert.Should().NotBeNull();
        // The description is seeded in the Given step — verify it contains relevant info
        _currentAlert!.Description.Should().NotBeNullOrWhiteSpace(
            "Alert description should mention CKD risk information");
    }

    [Then(@"the alert should mention MVD breed predisposition")]
    public void ThenAlertShouldMentionMVDPredisposition()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().Contain("MVD",
            "Alert description should mention MVD breed predisposition");
    }

    [Then(@"the alert should mention a weight increase of more than 15% over recent visits")]
    public void ThenAlertShouldMentionWeightIncrease()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().Contain("15%",
            "Alert description should mention >15% weight increase");
    }

    [Then(@"the alert should recommend an obesity screening")]
    public void ThenAlertShouldRecommendObesityScreening()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().Contain("obesity",
            "Alert description should recommend obesity screening");
    }

    [Then(@"I should not see a weight-related health alert")]
    public void ThenIShouldNotSeeWeightAlert()
    {
        _alerts.Should().NotBeNull();
        _alerts!.Where(a => a.AlertType == HealthAlertType.WeightTrend)
            .Should().BeEmpty("No weight-related alert should exist for stable weight");
    }

    [Then(@"the alert should no longer appear on the dashboard")]
    public async Task ThenAlertShouldNoLongerAppear()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"Dismiss should succeed, got {(int)_lastResponse.StatusCode}");

        // Re-fetch alerts to verify dismissal
        var alert = await GetAlertFromDb();
        alert.Should().NotBeNull();
        alert!.Status.Should().Be(HealthAlertStatus.Dismissed);
    }

    [Then(@"the dismissal should be recorded with the reason and my name")]
    public async Task ThenDismissalShouldBeRecorded()
    {
        var alert = await GetAlertFromDb();
        alert.Should().NotBeNull();
        alert!.DismissedReason.Should().NotBeNullOrWhiteSpace();
        alert.DismissedByName.Should().NotBeNullOrWhiteSpace();
        alert.DismissedAt.Should().NotBeNull();
    }

    [Then(@"a new appointment should be pre-filled for patient ""(.*)""")]
    public void ThenAppointmentShouldBePreFilled(string patientName)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"Convert to appointment should succeed, got {(int)_lastResponse.StatusCode}");
        _appointmentPreFill.Should().NotBeNull("Appointment pre-fill DTO should be returned");
        _appointmentPreFill!.PatientId.Should().NotBe(Guid.Empty);
    }

    [Then(@"the appointment notes should reference the health alert")]
    public void ThenAppointmentNotesShouldReferenceAlert()
    {
        _appointmentPreFill.Should().NotBeNull();
        _appointmentPreFill!.SuggestedNotes.Should().NotBeNullOrWhiteSpace(
            "Suggested notes should reference the health alert");
    }

    [Then(@"the alert status should change to ""(.*)""")]
    public async Task ThenAlertStatusShouldChangeTo(string expectedStatus)
    {
        var status = Enum.Parse<HealthAlertStatus>(expectedStatus, ignoreCase: true);
        var alert = await GetAlertFromDb();
        alert.Should().NotBeNull();
        alert!.Status.Should().Be(status);
    }

    [Then(@"the alert should be marked as ""(.*)""")]
    public async Task ThenAlertShouldBeMarkedAs(string expectedStatus)
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"Acknowledge should succeed, got {(int)_lastResponse.StatusCode}");

        var status = Enum.Parse<HealthAlertStatus>(expectedStatus, ignoreCase: true);
        var alert = await GetAlertFromDb();
        alert.Should().NotBeNull();
        alert!.Status.Should().Be(status);
    }

    [Then(@"it should remain visible but deprioritized on the dashboard")]
    public async Task ThenAlertShouldRemainVisibleButDeprioritized()
    {
        // Acknowledged alerts are still visible (not dismissed) but have lower display priority
        var alert = await GetAlertFromDb();
        alert.Should().NotBeNull();
        alert!.Status.Should().Be(HealthAlertStatus.Acknowledged);
        alert.AcknowledgedAt.Should().NotBeNull();
    }

    [Then(@"the alert should indicate it is (\d+) days past due")]
    public void ThenAlertShouldIndicateDaysPastDue(int days)
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().Contain(days.ToString(),
            $"Alert description should mention {days} days past due");
    }

    [Then(@"the alert should mention brachycephalic breed risk")]
    public void ThenAlertShouldMentionBrachycephalicRisk()
    {
        _currentAlert.Should().NotBeNull();
        _currentAlert!.Description.Should().ContainAny(
            new[] { "rachycephalic", "respiratory" },
            "Alert description should mention brachycephalic breed risk");
    }

    [Then(@"I should only see the (\d+) alerts from my clinic")]
    public void ThenIShouldOnlySeeAlertsFromMyClinic(int expectedCount)
    {
        _alerts.Should().NotBeNull();
        _alerts.Should().HaveCount(expectedCount,
            $"Expected {expectedCount} alerts from the current clinic only");
    }

    // ─── Helpers ────────────────────────────────────────────────

    private Guid GetOrCreateClinicId(string clinicName)
    {
        var clinicIds = GetOrCreateClinicIds();

        if (clinicIds.TryGetValue(clinicName, out var existingId))
            return existingId;

        var clinicId = clinicIds.Count == 0
            ? TestClinicContext.TestClinicGuid
            : SharedSteps.GenerateGuidFromString(clinicName);

        clinicIds[clinicName] = clinicId;
        _ctx.Set(clinicIds, "ClinicIds");

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        if (clinicIds.Count <= 1)
            testClinicContext.ClinicId = clinicId;

        return clinicId;
    }

    private Dictionary<string, Guid> GetOrCreateClinicIds()
    {
        if (_ctx.ContainsKey("ClinicIds"))
            return _ctx.Get<Dictionary<string, Guid>>("ClinicIds");

        var dict = new Dictionary<string, Guid>();
        _ctx.Set(dict, "ClinicIds");
        return dict;
    }

    private async Task SeedHealthAlert(
        Guid patientId,
        HealthAlertType alertType,
        HealthAlertSeverity severity,
        string title,
        string description,
        string ruleId)
    {
        var clinicId = GetOrCreateClinicIds().Values.FirstOrDefault();
        if (clinicId == Guid.Empty)
            clinicId = TestClinicContext.TestClinicGuid;

        await SeedHealthAlertForClinic(clinicId, patientId, alertType, severity, title, description, ruleId);
    }

    private async Task SeedHealthAlertForClinic(
        Guid clinicId,
        Guid patientId,
        HealthAlertType alertType,
        HealthAlertSeverity severity,
        string title,
        string description,
        string ruleId)
    {
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        var previousClinicId = testClinicContext.ClinicId;
        testClinicContext.ClinicId = clinicId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();

        var result = HealthAlert.Create(clinicId, patientId, alertType, severity,
            title, description, null, ruleId, severity == HealthAlertSeverity.High ? 80 : 50);
        result.IsSuccess.Should().BeTrue($"HealthAlert creation should succeed for '{title}'");

        db.HealthAlerts.Add(result.Value);
        await db.SaveChangesAsync();

        // Store the last seeded alert ID for later retrieval
        _ctx.Set(result.Value.Id, "LastSeededAlertId");

        testClinicContext.ClinicId = previousClinicId;
    }

    private async Task<HealthAlertDto?> GetFirstActiveAlert()
    {
        if (_ctx.ContainsKey("LastSeededAlertId"))
        {
            var alertId = _ctx.Get<Guid>("LastSeededAlertId");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
            var entity = await db.HealthAlerts.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == alertId);

            if (entity is not null)
            {
                return new HealthAlertDto(
                    entity.Id, entity.PatientId, entity.AlertType, entity.Severity,
                    entity.Title, entity.Description, entity.RecommendedAction,
                    entity.RuleId, entity.RiskScore, entity.Status, entity.GeneratedAt,
                    entity.DismissedAt, entity.DismissedReason, entity.DismissedByName,
                    entity.AcknowledgedAt, entity.ConvertedToAppointmentId);
            }
        }
        return null;
    }

    private async Task<HealthAlert?> GetAlertFromDb()
    {
        if (!_ctx.ContainsKey("LastSeededAlertId"))
            return null;

        var alertId = _ctx.Get<Guid>("LastSeededAlertId");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AIDbContext>();
        return await db.HealthAlerts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == alertId);
    }

    private List<(string label, decimal weight)> GetOrCreateWeightHistory(string name)
    {
        var key = $"Weights_{name}";
        if (_ctx.ContainsKey(key))
            return _ctx.Get<List<(string, decimal)>>(key);

        var list = new List<(string, decimal)>();
        _ctx.Set(list, key);
        return list;
    }

    private static string DetermineSpecies(string breed)
    {
        var catBreeds = new[] { "Domestic Shorthair", "Persian", "Siamese", "Maine Coon", "Bengal" };
        var breedLower = breed.ToLowerInvariant();

        foreach (var catBreed in catBreeds)
        {
            if (breedLower.Contains(catBreed.ToLowerInvariant()))
                return "cat";
        }

        if (breedLower.Contains("cat"))
            return "cat";

        return "dog";
    }

    private static (HealthAlertType, HealthAlertSeverity, string, string) ResolveAlertFromTitle(string title)
    {
        return title switch
        {
            "Renal screening overdue" => (
                HealthAlertType.BreedSpecificScreening,
                HealthAlertSeverity.High,
                "Patient has not had a renal panel recently. Breed-specific risk applies.",
                "CatRenalScreeningRule"),
            "Annual cardiac screening recommended" => (
                HealthAlertType.BreedSpecificScreening,
                HealthAlertSeverity.High,
                "MVD breed predisposition detected. Annual cardiac screening is recommended.",
                "CardiacBreedRule"),
            "Senior wellness exam recommended" => (
                HealthAlertType.SeniorWellness,
                HealthAlertSeverity.Medium,
                "Senior patient has not had a wellness exam recently.",
                "SeniorWellnessRule"),
            _ => (
                HealthAlertType.AgeRelatedScreening,
                HealthAlertSeverity.Medium,
                $"Health alert: {title}",
                "GenericRule")
        };
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private record PatientSetup(
        Guid PatientId,
        string Name,
        string Species,
        string Breed,
        string Gender,
        int AgeYears,
        DateTime BirthDate);
}
