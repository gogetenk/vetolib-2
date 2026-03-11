using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Dashboard;

[Binding]
[Scope(Feature = "Dashboard statistics")]
internal class DashboardSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private Guid _clinicId;
    private HttpResponseMessage? _lastResponse;
    private string? _errorResponseBody;

    // Stats response
    private DashboardStatsResponse? _stats;

    // Today appointments
    private List<TodayAppointmentResponse>? _todayAppointments;

    // Recent activity
    private List<ActivityResponse>? _recentActivity;

    // Analytics response
    private DashboardAnalyticsResponse? _analytics;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DashboardSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── GIVEN ──────────────────────────────────────────────────

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
        await AuthenticateAs(UserRole.Admin, "admin@dashboard-test.ae", "AdminPass1!");
    }

    [Given(@"I am authenticated as RECEPTIONIST")]
    public async Task GivenIAmAuthenticatedAsReceptionist()
    {
        await AuthenticateAs(UserRole.Receptionist, "receptionist@dashboard-test.ae", "RecepPass1!");
    }

    [Given(@"there are (\d+) appointments today")]
    public async Task GivenThereAreAppointmentsToday(int count)
    {
        // Create a vet user for the appointments
        var vetId = GenerateGuidFromString("dashboard-vet");
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var vetEmail = "vet@dashboard-test.ae";
        var existing = await authDb.Users.FindAsync(vetId);
        if (existing is null)
        {
            var vetResult = User.Create(_clinicId, vetEmail, "VetPass1!", UserRole.Vet, "TEST-VET-DASH-001");
            vetResult.IsSuccess.Should().BeTrue();
            typeof(Vetolib.Shared.Kernel.BaseEntity).GetProperty("Id")!.SetValue(vetResult.Value, vetId);
            authDb.Users.Add(vetResult.Value);
            await authDb.SaveChangesAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        for (int i = 0; i < count; i++)
        {
            var startTime = new TimeOnly(9 + i, 0);
            var animalName = $"Animal{i + 1}";
            var request = new CreateAppointmentRequest(
                VeterinarianId: vetId,
                VeterinarianName: "Dr Dashboard",
                AnimalId: GenerateGuidFromString(animalName),
                AnimalName: animalName,
                OwnerName: $"Owner{i + 1}",
                Date: today,
                StartTime: startTime,
                DurationMinutes: 30,
                Reason: "Checkup");

            var response = await _client.PostAsJsonAsync("/api/appointments", request);
            response.IsSuccessStatusCode.Should().BeTrue(
                $"Creating appointment {i + 1} failed: {await response.Content.ReadAsStringAsync()}");
        }
    }

    // ─── WHEN ───────────────────────────────────────────────────

    [When(@"I request dashboard stats")]
    public async Task WhenIRequestDashboardStats()
    {
        _lastResponse = await _client.GetAsync("/api/dashboard/stats");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _stats = await _lastResponse.Content.ReadFromJsonAsync<DashboardStatsResponse>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I request today's appointments")]
    public async Task WhenIRequestTodaysAppointments()
    {
        _lastResponse = await _client.GetAsync("/api/dashboard/today-appointments");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _todayAppointments = await _lastResponse.Content.ReadFromJsonAsync<List<TodayAppointmentResponse>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I request dashboard analytics")]
    public async Task WhenIRequestDashboardAnalytics()
    {
        _lastResponse = await _client.GetAsync("/api/dashboard/analytics");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _analytics = await _lastResponse.Content.ReadFromJsonAsync<DashboardAnalyticsResponse>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    [When(@"I request dashboard analytics without authentication")]
    public async Task WhenIRequestDashboardAnalyticsWithoutAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        _lastResponse = await _client.GetAsync("/api/dashboard/analytics");
        _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
    }

    [When(@"I request recent activity")]
    public async Task WhenIRequestRecentActivity()
    {
        _lastResponse = await _client.GetAsync("/api/dashboard/recent-activity");
        if (_lastResponse.IsSuccessStatusCode)
        {
            _recentActivity = await _lastResponse.Content.ReadFromJsonAsync<List<ActivityResponse>>(JsonOptions);
        }
        else
        {
            _errorResponseBody = await _lastResponse.Content.ReadAsStringAsync();
        }
    }

    // ─── THEN ───────────────────────────────────────────────────

    [Then(@"the response status is (\d+)")]
    public void ThenTheResponseStatusIs(int expectedStatus)
    {
        _lastResponse.Should().NotBeNull();
        ((int)_lastResponse!.StatusCode).Should().Be(expectedStatus);
    }

    [Then(@"the analytics include a revenue by month list")]
    public void ThenTheAnalyticsIncludeARevenueByMonthList()
    {
        _analytics.Should().NotBeNull();
        _analytics!.RevenueByMonth.Should().NotBeNull();
    }

    [Then(@"the analytics include an appointments by status list")]
    public void ThenTheAnalyticsIncludeAnAppointmentsByStatusList()
    {
        _analytics.Should().NotBeNull();
        _analytics!.AppointmentsByStatus.Should().NotBeNull();
    }

    [Then(@"I see appointments today count")]
    public void ThenISeeAppointmentsTodayCount()
    {
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _stats.Should().NotBeNull();
        _stats!.AppointmentsToday.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then(@"I see pending checkin count")]
    public void ThenISeePendingCheckinCount()
    {
        _stats.Should().NotBeNull();
        _stats!.PendingCheckin.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then(@"I see unpaid invoices total in AED")]
    public void ThenISeeUnpaidInvoicesTotalInAed()
    {
        _stats.Should().NotBeNull();
        _stats!.UnpaidInvoicesAed.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Then(@"I see total patients count")]
    public void ThenISeeTotalPatientsCount()
    {
        _stats.Should().NotBeNull();
        _stats!.TotalPatients.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then(@"I see (\d+) appointments sorted by time")]
    public void ThenISeeAppointmentsSortedByTime(int expectedCount)
    {
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _todayAppointments.Should().NotBeNull();
        _todayAppointments.Should().HaveCount(expectedCount);

        // Verify sorted by time (ascending)
        var times = _todayAppointments!.Select(a => a.ScheduledAt).ToList();
        times.Should().BeInAscendingOrder();
    }

    [Then(@"I receive a recent activity list")]
    public void ThenIReceiveARecentActivityList()
    {
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _recentActivity.Should().NotBeNull();
        // List may be empty (no activity yet) but must not be null
    }

    // ─── Helpers ────────────────────────────────────────────────

    private async Task AuthenticateAs(UserRole role, string email, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var existingUser = await authDb.Users.FindAsync(GenerateGuidFromString(email));
        if (existingUser is null)
        {
            var vetLicense = role == UserRole.Vet ? "TEST-VET-001" : null;
            var userResult = User.Create(_clinicId, email, password, role, vetLicense);
            userResult.IsSuccess.Should().BeTrue($"User creation for {email} failed");
            authDb.Users.Add(userResult.Value);
            await authDb.SaveChangesAsync();
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, password));
        loginResponse.IsSuccessStatusCode.Should().BeTrue($"Login for {email} failed");

        var authToken = await loginResponse.Content.ReadFromJsonAsync<AuthTokenDto>(JsonOptions);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken!.AccessToken);
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    // ─── Response DTOs ───────────────────────────────────────────

    private record DashboardStatsResponse(
        int AppointmentsToday,
        int PendingCheckin,
        decimal UnpaidInvoicesAed,
        int TotalPatients);

    private record TodayAppointmentResponse(
        string Id,
        string PatientName,
        string Species,
        string OwnerName,
        string VetName,
        string VetId,
        string Status,
        string ScheduledAt);

    private record ActivityResponse(
        string Id,
        string Type,
        string Message,
        string OccurredAt,
        string? RelatedId);

    private record DashboardAnalyticsResponse(
        IReadOnlyList<RevenueMonthResponse> RevenueByMonth,
        decimal NoShowRate,
        IReadOnlyList<SpeciesCountResponse> PatientsBySpecies,
        IReadOnlyList<StatusCountResponse> AppointmentsByStatus);

    private record RevenueMonthResponse(string Month, decimal Total, string Currency);
    private record SpeciesCountResponse(string Species, int Count);
    private record StatusCountResponse(string Status, int Count);
}
