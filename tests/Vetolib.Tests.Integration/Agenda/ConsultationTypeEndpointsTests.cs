using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for ConsultationType endpoints.
/// Validates HTTP contracts, auth/authz, and serialization.
/// </summary>
public sealed class ConsultationTypeEndpointsTests : IntegrationTestBase
{
    public ConsultationTypeEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/consultation-types ───────────────────────────────────

    [Fact]
    public async Task CreateConsultationType_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new CreateConsultationTypeRequest(
            Name: "General Checkup",
            DurationMinutes: 30,
            SortOrder: 1,
            RequiresVetSelection: true);

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/consultation-types", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ConsultationTypeDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("General Checkup");
        body.DurationMinutes.Should().Be(30);
        body.IsActive.Should().BeTrue();
        body.RequiresVetSelection.Should().BeTrue();
    }

    [Fact]
    public async Task CreateConsultationType_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new CreateConsultationTypeRequest("Test", 15);

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/consultation-types", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateConsultationType_NonAdminRole_Returns403()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new CreateConsultationTypeRequest("Vaccination", 20);

        // Act
        var response = await vetClient.PostAsJsonAsync("/api/v1/consultation-types", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/consultation-types ────────────────────────────────────

    [Fact]
    public async Task ListConsultationTypes_Authenticated_ReturnsSuccess()
    {
        // Arrange — create one first so the list is non-empty
        var adminClient = CreateAdminClient();
        var createRequest = new CreateConsultationTypeRequest("Dental Cleaning", 45, SortOrder: 2);
        await adminClient.PostAsJsonAsync("/api/v1/consultation-types", createRequest, JsonOptions);

        // Act
        var response = await adminClient.GetAsync("/api/v1/consultation-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ConsultationTypeDto>>(JsonOptions);
        items.Should().NotBeNull();
        items!.Should().Contain(ct => ct.Name == "Dental Cleaning");
    }

    [Fact]
    public async Task ListConsultationTypes_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/consultation-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/v1/consultation-types/{id} ───────────────────────────────

    [Fact]
    public async Task UpdateConsultationType_ValidRequest_ReturnsSuccess()
    {
        // Arrange — create one first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateConsultationTypeRequest("Surgery", 60, SortOrder: 3);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/consultation-types", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ConsultationTypeDto>(JsonOptions);

        var updateRequest = new UpdateConsultationTypeRequest(
            Name: "Minor Surgery",
            DurationMinutes: 45,
            SortOrder: 3,
            RequiresVetSelection: true);

        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/v1/consultation-types/{created!.Id}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ConsultationTypeDto>(JsonOptions);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Minor Surgery");
        updated.DurationMinutes.Should().Be(45);
    }

    [Fact]
    public async Task UpdateConsultationType_Unauthenticated_Returns401()
    {
        // Act
        var updateRequest = new UpdateConsultationTypeRequest("Test", 15, 0, false);
        var response = await Client.WithoutAuth().PutAsJsonAsync($"/api/v1/consultation-types/{Guid.NewGuid()}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/consultation-types/{id} ────────────────────────────

    [Fact]
    public async Task DeactivateConsultationType_ValidId_ReturnsSuccess()
    {
        // Arrange — create one first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateConsultationTypeRequest("Emergency Visit", 15, SortOrder: 10);
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/consultation-types", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ConsultationTypeDto>(JsonOptions);

        // Act
        var response = await adminClient.DeleteAsync($"/api/v1/consultation-types/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateConsultationType_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().DeleteAsync($"/api/v1/consultation-types/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
