using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Vetolib.Auth.Application.Domain;
using Vetolib.Auth.Contracts;
using Vetolib.Auth.Infrastructure;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Tests.Acceptance.Support;

namespace Vetolib.Tests.Acceptance.StepDefinitions.Auth;

[Binding]
[Scope(Feature = "RBAC matrix — role-based access control")]
internal class RbacSteps
{
    private readonly ScenarioContext _ctx;
    private HttpClient _client = null!;
    private TestWebApplicationFactory _factory = null!;
    private HttpResponseMessage _response = null!;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public RbacSteps(ScenarioContext ctx)
    {
        _ctx = ctx;
    }

    [BeforeScenario(Order = 1)]
    public void SetupClient()
    {
        _factory = _ctx.Get<TestWebApplicationFactory>();
        _client = _ctx.Get<HttpClient>();
    }

    // ─── WHEN Steps ──────────────────────────────────────────────

    [When(@"I attempt to create an appointment")]
    public async Task WhenIAttemptToCreateAnAppointment()
    {
        var clinicId = GetClinicId();

        // Ensure a vet user exists so the appointment can have a valid veterinarian reference
        var vetId = await EnsureVetUserExists(clinicId);

        var body = new
        {
            VeterinarianId = vetId,
            VeterinarianName = "Dr. Test",
            AnimalId = Guid.NewGuid(),
            AnimalName = "Fluffy",
            OwnerName = "Mohammed Al-Rashid",
            Date = "2026-06-01",
            StartTime = "09:00:00",
            DurationMinutes = 30,
            Reason = "Annual consultation"
        };

        _response = await _client.PostAsJsonAsync("/api/v1/appointments", body);
    }

    [When(@"I attempt to add a medical record")]
    public async Task WhenIAttemptToAddAMedicalRecord()
    {
        var clinicId = GetClinicId();

        // Create a patient in the DB so we have a valid patientId
        var patientId = await EnsurePatientExists(clinicId);

        var body = new
        {
            Diagnosis = "Test diagnosis",
            Treatment = "Test treatment"
        };

        _response = await _client.PostAsJsonAsync($"/api/v1/patients/{patientId}/records", body);
    }

    [When(@"I attempt to create an invoice")]
    public async Task WhenIAttemptToCreateAnInvoice()
    {
        var body = new
        {
            AnimalId = Guid.NewGuid(),
            ItemDescription = "Consultation",
            ItemUnitPrice = 150.00m
        };

        _response = await _client.PostAsJsonAsync("/api/v1/invoices", body);
    }

    [When(@"I attempt to add a prescription to a medical record")]
    public async Task WhenIAttemptToAddAPrescriptionToAMedicalRecord()
    {
        var clinicId = GetClinicId();

        // We need a patient + a medical record owned by a vet
        var (patientId, recordId) = await EnsurePatientAndRecordExist(clinicId);

        var body = new
        {
            Medication = "Amoxicillin 500mg",
            Dosage = "2x daily for 7 days"
        };

        _response = await _client.PostAsJsonAsync(
            $"/api/v1/patients/{patientId}/records/{recordId}/prescriptions",
            body);
    }

    // ─── THEN Steps ──────────────────────────────────────────────

    [Then(@"the system returns 403")]
    public void ThenTheSystemReturns403()
    {
        _response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"Expected 403 Forbidden but got {(int)_response.StatusCode} {_response.StatusCode}");
    }

    [Then(@"the system accepts the request")]
    public void ThenTheSystemAcceptsTheRequest()
    {
        // 200, 201 or 422 (validation error) are all acceptable — the authz check passed
        var statusCode = (int)_response.StatusCode;
        statusCode.Should().NotBe(403, $"Request should not be forbidden, but got {statusCode}");
        statusCode.Should().NotBe(401, $"Request should not be unauthorized, but got {statusCode}");
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Guid GetClinicId()
    {
        if (_ctx.ContainsKey("ClinicIds"))
        {
            var clinicIds = _ctx.Get<Dictionary<string, Guid>>("ClinicIds");
            return clinicIds.Values.First();
        }

        // Fallback: use the test clinic context
        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        return testClinicContext.ClinicId;
    }

    private async Task<Guid> EnsureVetUserExists(Guid clinicId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        var vetEmail = "vet-for-rbac@test.ae";
        var existing = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == vetEmail);

        if (existing is not null)
            return existing.Id;

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        var vet = User.Create(clinicId, vetEmail, "SecurePass1", UserRole.Vet, "VET-RBAC-001");
        vet.IsSuccess.Should().BeTrue();
        db.Users.Add(vet.Value);
        await db.SaveChangesAsync();
        return vet.Value.Id;
    }

    private async Task<Guid> EnsurePatientExists(Guid clinicId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        var patient = Patient.Create(clinicId, "Max", Species.Dog, "Labrador", new DateOnly(2020, 1, 1));
        patient.IsSuccess.Should().BeTrue();
        db.Patients.Add(patient.Value);
        await db.SaveChangesAsync();
        return patient.Value.Id;
    }

    private async Task<(Guid patientId, Guid recordId)> EnsurePatientAndRecordExist(Guid clinicId)
    {
        var patientId = await EnsurePatientExists(clinicId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalRecordsDbContext>();

        var testClinicContext = _factory.Services.GetRequiredService<TestClinicContext>();
        testClinicContext.ClinicId = clinicId;

        var record = MedicalRecord.Create(clinicId, patientId, "Test diagnostic", "Test treatment", "Dr. Omar", DateTime.UtcNow);
        record.IsSuccess.Should().BeTrue();
        db.MedicalRecords.Add(record.Value);
        await db.SaveChangesAsync();
        return (patientId, record.Value.Id);
    }

    private static Guid GenerateGuidFromString(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
