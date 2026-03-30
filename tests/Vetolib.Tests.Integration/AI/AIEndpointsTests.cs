using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.AI.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.AI;

/// <summary>
/// Integration tests for AI module endpoints.
/// Contract testing: verifies HTTP status codes, auth requirements, and response shapes.
/// </summary>
public sealed class AIEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AIEndpointsTests(VetolibWebApplicationFactory factory) : base(factory) { }

    // ── POST /api/v1/ai/triage ────────────────────────────────────────────

    [Fact]
    public async Task TriageSymptoms_ValidRequest_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new
        {
            Symptoms = "Lethargy, loss of appetite, vomiting",
            Species = "Dog",
            Breed = "Golden Retriever",
            AgeMonths = 36,
            WeightKg = 30.0m
        };

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/ai/triage", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOpts);
        body.Should().NotBeNull();
        body!.TriageId.Should().NotBeEmpty();
        body.Reasoning.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task TriageSymptoms_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new
        {
            Symptoms = "Coughing",
            Species = "Cat"
        };

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/ai/triage", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/v1/ai/triage/{id}/accept ─────────────────────────────────

    [Fact]
    public async Task AcceptTriage_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PutAsync(
            $"/api/v1/ai/triage/{Guid.NewGuid()}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AcceptTriage_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var vetClient = CreateVetClient();

        // Act
        var response = await vetClient.PutAsync(
            $"/api/v1/ai/triage/{Guid.NewGuid()}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptTriage_AfterTriageCreated_Returns200()
    {
        // Arrange — create a triage first
        var vetClient = CreateVetClient();
        var triageRequest = new
        {
            Symptoms = "High fever, dehydration",
            Species = "Cat",
            Breed = "Siamese",
            AgeMonths = 24,
            WeightKg = 4.5m
        };
        var triageResponse = await vetClient.PostAsJsonAsync("/api/v1/ai/triage", triageRequest, JsonOpts);
        var triage = await triageResponse.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOpts);

        // Act
        var response = await vetClient.PutAsync(
            $"/api/v1/ai/triage/{triage!.TriageId}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── PUT /api/v1/ai/triage/{id}/override ───────────────────────────────

    [Fact]
    public async Task OverrideTriage_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new { NewSeverity = "Emergency" };

        // Act
        var response = await Client.WithoutAuth().PutAsJsonAsync(
            $"/api/v1/ai/triage/{Guid.NewGuid()}/override", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OverrideTriage_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new { NewSeverity = "Emergency" };

        // Act
        var response = await vetClient.PutAsJsonAsync(
            $"/api/v1/ai/triage/{Guid.NewGuid()}/override", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OverrideTriage_AfterTriageCreated_Returns200()
    {
        // Arrange — create a triage first
        var vetClient = CreateVetClient();
        var triageRequest = new
        {
            Symptoms = "Limping, swollen paw",
            Species = "Dog",
            Breed = "Labrador",
            AgeMonths = 60,
            WeightKg = 32.0m
        };
        var triageResponse = await vetClient.PostAsJsonAsync("/api/v1/ai/triage", triageRequest, JsonOpts);
        var triage = await triageResponse.Content.ReadFromJsonAsync<TriageSuggestionDto>(JsonOpts);

        var request = new { NewSeverity = "Emergency" };

        // Act
        var response = await vetClient.PutAsJsonAsync(
            $"/api/v1/ai/triage/{triage!.TriageId}/override", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/v1/ai/no-show-prediction/{appointmentId} ─────────────────

    [Fact]
    public async Task PredictNoShow_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/ai/no-show-prediction/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PredictNoShow_ValidRequest_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var appointmentId = Guid.NewGuid();

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/ai/no-show-prediction/{appointmentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<NoShowPredictionDto>(JsonOpts);
        body.Should().NotBeNull();
        body!.AppointmentId.Should().Be(appointmentId);
        body.NoShowProbability.Should().BeInRange(0f, 1f);
    }

    // ── POST /api/v1/ai/no-show-predictions/batch ─────────────────────────

    [Fact]
    public async Task PredictNoShowBatch_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd") };

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/ai/no-show-predictions/batch", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PredictNoShowBatch_ValidDate_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new { Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        // Act
        var response = await adminClient.PostAsJsonAsync(
            "/api/v1/ai/no-show-predictions/batch", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── POST /api/v1/ai/soap-notes ────────────────────────────────────────

    [Fact]
    public async Task GenerateSoapNotes_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new
        {
            Species = "Dog",
            Breed = "German Shepherd",
            PatientName = "Rex",
            Symptoms = "Coughing, nasal discharge",
            Vitals = "T: 39.5C, HR: 120, RR: 30",
            Diagnosis = "Upper respiratory infection",
            TreatmentPlan = "Antibiotics course 7 days",
            Prescriptions = new[] { "Amoxicillin 250mg BID" }
        };

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/ai/soap-notes", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateSoapNotes_ValidRequest_Returns200()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new
        {
            Species = "Cat",
            Breed = "Persian",
            PatientName = "Luna",
            Symptoms = "Sneezing, watery eyes",
            Vitals = "T: 38.8C, HR: 180, RR: 28",
            Diagnosis = "Feline herpesvirus conjunctivitis",
            TreatmentPlan = "Antiviral eye drops, supportive care",
            Prescriptions = new[] { "Idoxuridine 0.1% ophthalmic drops TID" }
        };

        // Act
        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/ai/soap-notes", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SoapNoteDto>(JsonOpts);
        body.Should().NotBeNull();
        body!.Subjective.Should().NotBeNullOrWhiteSpace();
        body.Objective.Should().NotBeNullOrWhiteSpace();
        body.Assessment.Should().NotBeNullOrWhiteSpace();
        body.Plan.Should().NotBeNullOrWhiteSpace();
    }

    // ── POST /api/v1/ai/check-interactions ────────────────────────────────

    [Fact]
    public async Task CheckInteractions_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new
        {
            PatientId = Guid.NewGuid(),
            DrugCatalogEntryId = Guid.NewGuid(),
            DosageAmount = 10.0m
        };

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync(
            "/api/v1/ai/check-interactions", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckInteractions_NonExistentDrug_ReturnsNotFound()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new
        {
            PatientId = Guid.NewGuid(),
            DrugCatalogEntryId = Guid.NewGuid(),
            DosageAmount = 10.0m
        };

        // Act
        var response = await vetClient.PostAsJsonAsync(
            "/api/v1/ai/check-interactions", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/v1/ai/health-alerts ──────────────────────────────────────

    [Fact]
    public async Task GetHealthAlerts_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/ai/health-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHealthAlerts_Authenticated_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/ai/health-alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<HealthAlertDto>>(JsonOpts);
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealthAlerts_WithSeverityFilter_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync("/api/v1/ai/health-alerts?severity=High");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── GET /api/v1/ai/health-alerts/patient/{patientId} ──────────────────

    [Fact]
    public async Task GetPatientHealthAlerts_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync(
            $"/api/v1/ai/health-alerts/patient/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatientHealthAlerts_ValidPatient_Returns200()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.GetAsync(
            $"/api/v1/ai/health-alerts/patient/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<HealthAlertDto>>(JsonOpts);
        body.Should().NotBeNull();
    }

    // ── POST /api/v1/ai/health-alerts/generate ────────────────────────────

    [Fact]
    public async Task GenerateHealthAlerts_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PostAsync(
            "/api/v1/ai/health-alerts/generate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateHealthAlerts_AsVet_Returns200()
    {
        // Arrange
        var vetClient = CreateVetClient();

        // Act
        var response = await vetClient.PostAsync(
            "/api/v1/ai/health-alerts/generate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── PATCH /api/v1/ai/health-alerts/{id}/dismiss ───────────────────────

    [Fact]
    public async Task DismissHealthAlert_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new { Reason = "Duplicate alert" };

        // Act
        var response = await Client.WithoutAuth().PatchAsJsonAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/dismiss", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DismissHealthAlert_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new { Reason = "Not applicable" };

        // Act
        var response = await vetClient.PatchAsJsonAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/dismiss", request, JsonOpts);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /api/v1/ai/health-alerts/{id}/acknowledge ───────────────────

    [Fact]
    public async Task AcknowledgeHealthAlert_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PatchAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/acknowledge", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AcknowledgeHealthAlert_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminClient = CreateAdminClient();

        // Act
        var response = await adminClient.PatchAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/acknowledge", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /api/v1/ai/health-alerts/{id}/convert-to-appointment ─────────

    [Fact]
    public async Task ConvertAlertToAppointment_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().PostAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/convert-to-appointment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConvertAlertToAppointment_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var vetClient = CreateVetClient();

        // Act
        var response = await vetClient.PostAsync(
            $"/api/v1/ai/health-alerts/{Guid.NewGuid()}/convert-to-appointment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
