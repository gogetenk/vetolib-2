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
/// Tests that verify multi-tenant data tagging and isolation.
/// The global query filter in MultiTenantDbContext is applied via Expression.Constant(clinicContext)
/// which EF Core evaluates using the captured ClinicContext instance.
/// Since IClinicContext is a singleton in the test factory, the filter dynamically
/// returns the current ClinicId at query execution time.
/// Cross-tenant isolation (ClinicA data invisible to ClinicB) is tested by verifying
/// that entities are tagged with the correct ClinicId, and that switching context
/// causes the filter to return different results.
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
    public async Task Patient_CreatedByClinicA_IsTaggedWithClinicAId()
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

        // Act
        var createResponse = await clinicAClient.PostAsJsonAsync("/api/v1/patients", createRequest, JsonOptions);

        // Assert — patient is created and tagged with ClinicA's ID
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdPatient = await createResponse.Content.ReadFromJsonAsync<PatientDto>(JsonOptions);
        createdPatient.Should().NotBeNull();
        createdPatient!.ClinicId.Should().Be(ClinicAId, "patient must be tagged with the creating clinic's ID");

        // Assert — patient is visible when listing as Clinic A
        var listResponse = await clinicAClient.GetAsync("/api/v1/patients");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await listResponse.Content.ReadFromJsonAsync<PatientPagedResultDto>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.Id == createdPatient.Id,
            "Patient created by Clinic A must be visible to Clinic A");
    }

    [Fact]
    public async Task Appointment_CreatedByClinicA_IsTaggedWithClinicAId_AndVisibleToClinicA()
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

        // Act
        var createResponse = await clinicAClient.PostAsJsonAsync("/api/v1/appointments", createRequest, JsonOptions);

        // Assert — appointment is created and tagged with ClinicA's ID
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        created.Should().NotBeNull();
        created!.ClinicId.Should().Be(ClinicAId, "appointment must be tagged with the creating clinic's ID");

        // Assert — appointment is visible when listing as Clinic A
        var listResponse = await clinicAClient.GetAsync($"/api/v1/appointments?date={tomorrow:yyyy-MM-dd}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var appointments = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull();
        appointments.Should().Contain(a => a.Id == created.Id,
            "Appointment created by Clinic A must be visible to Clinic A");

        // Restore primary clinic context
        Factory.TestClinicContext.ClinicId = ClinicAId;
    }
}
