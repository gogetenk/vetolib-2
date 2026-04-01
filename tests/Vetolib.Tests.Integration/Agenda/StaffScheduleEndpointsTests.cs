using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for StaffSchedule endpoints.
/// Validates HTTP contracts, auth/authz, and serialization.
/// </summary>
public sealed class StaffScheduleEndpointsTests : IntegrationTestBase
{
    public StaffScheduleEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/schedule ─────────────────────────────────────────────

    [Fact]
    public async Task CreateStaffSchedule_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var staffId = Guid.NewGuid();
        var scheduleDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var request = new CreateStaffScheduleRequest(
            UserId: staffId,
            UserName: "Dr. Mohammed Al-Hashimi",
            Date: scheduleDate,
            StartTime: TimeOnly.Parse("08:00"),
            EndTime: TimeOnly.Parse("16:00"),
            ShiftType: ShiftType.FullDay,
            IsAvailable: true);

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/schedule", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StaffScheduleDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.UserId.Should().Be(staffId);
        body.UserName.Should().Be("Dr. Mohammed Al-Hashimi");
        body.Date.Should().Be(scheduleDate);
        body.ShiftType.Should().Be(ShiftType.FullDay);
        body.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateStaffSchedule_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new CreateStaffScheduleRequest(
            Guid.NewGuid(), "Dr. Test",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeOnly.Parse("09:00"), TimeOnly.Parse("17:00"),
            ShiftType.Morning, true);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/schedule", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateStaffSchedule_NonAdminRole_Returns403()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new CreateStaffScheduleRequest(
            Guid.NewGuid(), "Dr. Fatima Al-Marzouqi",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            TimeOnly.Parse("09:00"), TimeOnly.Parse("13:00"),
            ShiftType.Morning, true);

        // Act
        var response = await vetClient.PostAsJsonAsync("/api/v1/schedule", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/schedule ──────────────────────────────────────────────

    [Fact]
    public async Task ListStaffSchedules_WithDateRange_ReturnsSuccess()
    {
        // Arrange — create a schedule entry first
        var adminClient = CreateAdminClient();
        var scheduleDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

        var createRequest = new CreateStaffScheduleRequest(
            Guid.NewGuid(), "Dr. Sara Al-Nuaimi", scheduleDate,
            TimeOnly.Parse("08:00"), TimeOnly.Parse("12:00"),
            ShiftType.Morning, true);
        await adminClient.PostAsJsonAsync("/api/v1/schedule", createRequest, JsonOptions);

        var from = scheduleDate.AddDays(-1);
        var to = scheduleDate.AddDays(1);

        // Act
        var response = await adminClient.GetAsync($"/api/v1/schedule?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<StaffScheduleDto>>(JsonOptions);
        items.Should().NotBeNull();
        items!.Should().Contain(s => s.UserName == "Dr. Sara Al-Nuaimi");
    }

    [Fact]
    public async Task ListStaffSchedules_Unauthenticated_Returns401()
    {
        // Act
        var from = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = from.AddDays(7);
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/schedule?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/schedule/me ───────────────────────────────────────────

    [Fact]
    public async Task GetMySchedule_Authenticated_ReturnsSuccess()
    {
        // Arrange
        var vetClient = CreateVetClient();

        // Act
        var response = await vetClient.GetAsync("/api/v1/schedule/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMySchedule_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/schedule/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/schedule/{id} ──────────────────────────────────────

    [Fact]
    public async Task DeleteStaffSchedule_ValidId_ReturnsSuccess()
    {
        // Arrange — create one first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateStaffScheduleRequest(
            Guid.NewGuid(), "Dr. Khalid Al-Mansouri",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            TimeOnly.Parse("14:00"), TimeOnly.Parse("18:00"),
            ShiftType.Afternoon, true);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/schedule", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<StaffScheduleDto>(JsonOptions);

        // Act
        var response = await adminClient.DeleteAsync($"/api/v1/schedule/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteStaffSchedule_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().DeleteAsync($"/api/v1/schedule/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/v1/schedule/availability ─────────────────────────────────

    [Fact]
    public async Task GetStaffAvailability_WithDate_ReturnsSuccess()
    {
        // Arrange — create an available schedule entry
        var adminClient = CreateAdminClient();
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4));

        var createRequest = new CreateStaffScheduleRequest(
            Guid.NewGuid(), "Dr. Layla Al-Hamdan", targetDate,
            TimeOnly.Parse("08:00"), TimeOnly.Parse("16:00"),
            ShiftType.FullDay, true);
        await adminClient.PostAsJsonAsync("/api/v1/schedule", createRequest, JsonOptions);

        // Act
        var response = await adminClient.GetAsync($"/api/v1/schedule/availability?date={targetDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetStaffAvailability_Unauthenticated_Returns401()
    {
        // Act
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await Client.WithoutAuth().GetAsync($"/api/v1/schedule/availability?date={date:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
