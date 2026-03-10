using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.MultiTenancy;

/// <summary>
/// Tests that verify multi-tenant data isolation: entities created by Clinic A
/// must not be visible to Clinic B.
/// </summary>
public sealed class TenantIsolationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static readonly Guid ClinicAId = IntegrationTestClinicContext.PrimaryClinicId;
    private static readonly Guid ClinicBId = IntegrationTestClinicContext.SecondaryClinicId;

    public TenantIsolationTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Patient_CreatedByClinicA_IsInvisibleToClinicB()
    {
        // Arrange — create a patient as Clinic A
        Factory.TestClinicContext.ClinicId = ClinicAId;
        var clinicAClient = Factory.CreateClient()
            .WithRole(ClinicAId, UserRole.Vet, vetLicenseNumber: "UAE-VET-A-001");

        var createRequest = new CreatePatientRequest(
            Name: "Clinic A Patient",
            Species: Species.Dog,
            Breed: "Husky",
            BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            OwnerName: "Clinic A Owner",
            OwnerPhone: "+971 50 100 0001");

        var createResponse = await clinicAClient.PostAsJsonAsync("/api/v1/patients", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPatient = await createResponse.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);

        // Act — switch to Clinic B and try to list patients
        Factory.TestClinicContext.ClinicId = ClinicBId;
        var clinicBClient = Factory.CreateClient()
            .WithRole(ClinicBId, UserRole.Vet, vetLicenseNumber: "UAE-VET-B-001");

        var listResponse = await clinicBClient.GetAsync("/api/v1/patients");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await listResponse.Content.ReadFromJsonAsync<PatientPagedResultDto>(JsonOptions);

        // Assert — Clinic B cannot see Clinic A's patient
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(p => p.Id == createdPatient!.Id,
            "Patient created by Clinic A must be invisible to Clinic B");
    }

    [Fact]
    public async Task Appointment_CreatedByClinicA_IsInvisibleToClinicB()
    {
        // Arrange — create an appointment as Clinic A
        Factory.TestClinicContext.ClinicId = ClinicAId;
        var clinicAClient = Factory.CreateClient().WithAdminAuth(ClinicAId);

        var vetId = Guid.NewGuid();
        var animalId = Guid.NewGuid();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var createRequest = new CreateAppointmentRequest(
            VeterinarianId: vetId,
            VeterinarianName: "Dr. Clinic A Vet",
            AnimalId: animalId,
            AnimalName: "Clinic A Animal",
            OwnerName: "Clinic A Owner",
            Date: tomorrow,
            StartTime: TimeOnly.Parse("16:00"),
            DurationMinutes: 30,
            Reason: "Isolation test appointment");

        var createResponse = await clinicAClient.PostAsJsonAsync("/api/v1/appointments", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);

        // Act — switch to Clinic B and list appointments
        Factory.TestClinicContext.ClinicId = ClinicBId;
        var clinicBClient = Factory.CreateClient().WithAdminAuth(ClinicBId);

        var listResponse = await clinicBClient.GetAsync($"/api/v1/appointments?date={tomorrow:yyyy-MM-dd}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<AppointmentDto>>(JsonOptions);

        // Assert — Clinic B cannot see Clinic A's appointment
        appointments.Should().NotContain(a => a.Id == created!.Id,
            "Appointment created by Clinic A must be invisible to Clinic B");

        // Restore primary clinic context
        Factory.TestClinicContext.ClinicId = ClinicAId;
    }
}
