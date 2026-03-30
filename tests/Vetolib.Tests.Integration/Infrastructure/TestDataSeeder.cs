using System.Net.Http.Json;
using System.Text.Json;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Vetolib.Billing.Contracts;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Fluent seeder for creating test data via HTTP (goes through the full stack).
/// Uses realistic UAE names, AED amounts, and Asia/Dubai timezone.
/// </summary>
public sealed class TestDataSeeder
{
    private readonly HttpClient _adminClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public TestDataSeeder(HttpClient adminClient)
    {
        _adminClient = adminClient;
    }

    public async Task<RegisterClinicResponse> RegisterClinicAsync(
        string clinicName = "Al Barsha Veterinary Clinic",
        string email = "admin@albarsha-vet.ae",
        string password = "SecureAdmin1!",
        string phone = "+971 4 123 4567",
        string country = "UAE")
    {
        var request = new RegisterClinicRequest(clinicName, email, password, phone, country);
        var response = await _adminClient.PostAsJsonAsync("/api/v1/clinics/register", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegisterClinicResponse>(JsonOptions))!;
    }

    public async Task<PatientDto> CreatePatientAsync(
        string name = "Max",
        Species species = Species.Dog,
        string breed = "Golden Retriever",
        DateOnly? birthDate = null,
        string ownerName = "Ahmed Al-Rashid",
        string ownerPhone = "+971 50 123 4567")
    {
        var request = new CreatePatientRequest(
            name,
            species,
            breed,
            birthDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
            ownerName,
            ownerPhone);

        var response = await _adminClient.PostAsJsonAsync("/api/v1/patients", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions))!;
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(
        Guid veterinarianId,
        Guid animalId,
        DateOnly? date = null,
        TimeOnly? startTime = null,
        string veterinarianName = "Dr. Mohammed Al-Hashimi",
        string animalName = "Max",
        string ownerName = "Ahmed Al-Rashid",
        int durationMinutes = 30,
        string? reason = "Annual checkup")
    {
        var request = new CreateAppointmentRequest(
            veterinarianId,
            veterinarianName,
            animalId,
            animalName,
            ownerName,
            null,
            date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            startTime ?? TimeOnly.Parse("10:00"),
            durationMinutes,
            reason);

        var response = await _adminClient.PostAsJsonAsync("/api/v1/appointments", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions))!;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(
        Guid animalId,
        string itemDescription = "Consultation générale",
        decimal itemUnitPrice = 350.00m)
    {
        var request = new CreateInvoiceRequest(animalId, itemDescription, itemUnitPrice);
        var response = await _adminClient.PostAsJsonAsync("/api/v1/invoices", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InvoiceDto>(JsonOptions))!;
    }
}
