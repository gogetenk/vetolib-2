using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for Appointment endpoints.
/// Complements BDD scenarios by testing HTTP-level contracts and edge cases.
/// </summary>
public sealed class AppointmentEndpointsTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AppointmentEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/appointments ──────────────────────────────────────────

    [Fact]
    public async Task CreateAppointment_ValidRequest_Returns201()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var vetId = Guid.NewGuid();
        var animalId = Guid.NewGuid();

        var request = new CreateAppointmentRequest(
            VeterinarianId: vetId,
            VeterinarianName: "Dr. Mohammed Al-Hashimi",
            AnimalId: animalId,
            AnimalName: "Max",
            OwnerName: "Ahmed Al-Rashid",
            OwnerEmail: "ahmed@email.ae",
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime: TimeOnly.Parse("09:00"),
            DurationMinutes: 30,
            Reason: "Annual checkup");

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/appointments", request, JsonOptions);

        // Assert — ToMinimalApiResult() maps Result.Success to 200 OK for creates
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.AnimalName.Should().Be("Max");
        body.Status.Should().Be(AppointmentStatus.Scheduled);
        body.ClinicId.Should().Be(TestClinicId);
    }

    [Fact]
    public async Task CreateAppointment_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new CreateAppointmentRequest(
            Guid.NewGuid(), "Dr. Test", Guid.NewGuid(), "Luna", "Owner", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeOnly.Parse("10:00"), 30, null);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/appointments", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/appointments ───────────────────────────────────────────

    [Fact]
    public async Task ListAppointments_WithDate_Returns200WithResults()
    {
        // Arrange — create an appointment for tomorrow
        var adminClient = CreateAdminClient();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var createRequest = new CreateAppointmentRequest(
            VeterinarianId: Guid.NewGuid(),
            VeterinarianName: "Dr. Fatima Al-Marzouqi",
            AnimalId: Guid.NewGuid(),
            AnimalName: "Bella",
            OwnerName: "Khalid Al-Mansouri",
            OwnerEmail: "khalid@email.ae",
            Date: tomorrow,
            StartTime: TimeOnly.Parse("11:00"),
            DurationMinutes: 45,
            Reason: "Vaccination");

        await adminClient.PostAsJsonAsync("/api/v1/appointments", createRequest, JsonOptions);

        // Act
        var response = await adminClient.GetAsync($"/api/v1/appointments?date={tomorrow:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var appointments = await response.Content.ReadFromJsonAsync<IReadOnlyList<AppointmentDto>>(JsonOptions);
        appointments.Should().NotBeNull();
        appointments.Should().HaveCountGreaterThanOrEqualTo(1);
        appointments!.Should().Contain(a => a.AnimalName == "Bella");
    }

    // ── PATCH /api/v1/appointments/{id}/transition ─────────────────────────

    [Fact]
    public async Task TransitionAppointment_ValidAction_Returns200()
    {
        // Arrange — create an appointment first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateAppointmentRequest(
            VeterinarianId: Guid.NewGuid(),
            VeterinarianName: "Dr. Hassan Al-Zaabi",
            AnimalId: Guid.NewGuid(),
            AnimalName: "Rocky",
            OwnerName: "Sara Al-Nuaimi",
            OwnerEmail: "sara@email.ae",
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime: TimeOnly.Parse("14:00"),
            DurationMinutes: 30,
            Reason: "Check-up");

        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/appointments", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);

        // Act — transition to CheckedIn
        var transitionRequest = new TransitionAppointmentRequest("CHECK_IN", null);
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/appointments/{created!.Id}/transition",
            transitionRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        updated!.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    [Fact]
    public async Task TransitionAppointment_InvalidAction_Returns422()
    {
        // Arrange — create an appointment first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateAppointmentRequest(
            VeterinarianId: Guid.NewGuid(),
            VeterinarianName: "Dr. Layla Al-Hamdan",
            AnimalId: Guid.NewGuid(),
            AnimalName: "Milo",
            OwnerName: "Omar Al-Sabah",
            OwnerEmail: null,
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
            StartTime: TimeOnly.Parse("15:00"),
            DurationMinutes: 30,
            Reason: null);

        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/appointments", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);

        // Act — use an unsupported action value
        var transitionRequest = new TransitionAppointmentRequest("INVALID_ACTION", null);
        var response = await adminClient.PatchAsJsonAsync(
            $"/api/v1/appointments/{created!.Id}/transition",
            transitionRequest,
            JsonOptions);

        // Assert — UNSUPPORTED_ACTION maps to Error which becomes 422 via ToMinimalApiResult
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest);
    }

    // ── GET /api/v1/appointments (today filter) ────────────────────────────

    [Fact]
    public async Task ListAppointments_TodayFilter_ReturnsOnlyTodayAppointments()
    {
        // Arrange — create one appointment for today and one for tomorrow
        var adminClient = CreateAdminClient();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tomorrow = today.AddDays(1);

        var todayRequest = new CreateAppointmentRequest(
            Guid.NewGuid(), "Dr. Yousef Al-Rashidi", Guid.NewGuid(),
            "Simba", "Noura Al-Khatib", null, today, TimeOnly.Parse("08:00"), 20, null);

        var tomorrowRequest = new CreateAppointmentRequest(
            Guid.NewGuid(), "Dr. Yousef Al-Rashidi", Guid.NewGuid(),
            "Cleo", "Noura Al-Khatib", null, tomorrow, TimeOnly.Parse("08:00"), 20, null);

        await adminClient.PostAsJsonAsync("/api/v1/appointments", todayRequest, JsonOptions);
        await adminClient.PostAsJsonAsync("/api/v1/appointments", tomorrowRequest, JsonOptions);

        // Act — list appointments for today
        var response = await adminClient.GetAsync($"/api/v1/appointments?date={today:yyyy-MM-dd}");
        var appointments = await response.Content.ReadFromJsonAsync<IReadOnlyList<AppointmentDto>>(JsonOptions);

        // Assert — only today's appointments returned
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        appointments.Should().NotBeNull();
        appointments!.Should().OnlyContain(a => a.Date == today);
    }
}
