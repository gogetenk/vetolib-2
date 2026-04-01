using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Agenda.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Agenda;

/// <summary>
/// Integration tests for FollowUpRule endpoints.
/// Validates HTTP contracts, auth/authz, and serialization.
/// </summary>
public sealed class FollowUpRuleEndpointsTests : IntegrationTestBase
{
    public FollowUpRuleEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/follow-up-rules ──────────────────────────────────────

    [Fact]
    public async Task CreateFollowUpRule_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminClient = CreateAdminClient();
        var request = new CreateFollowUpRuleRequest(
            ConsultationType: "Vaccination",
            FollowUpDays: 30,
            FollowUpReason: "Post-vaccination checkup");

        // Act
        var response = await adminClient.PostAsJsonAsync("/api/v1/follow-up-rules", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FollowUpRuleDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
        body.ConsultationType.Should().Be("Vaccination");
        body.FollowUpDays.Should().Be(30);
        body.FollowUpReason.Should().Be("Post-vaccination checkup");
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateFollowUpRule_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new CreateFollowUpRuleRequest("Surgery", 14, "Post-op follow-up");

        // Act
        var response = await Client.WithoutAuth().PostAsJsonAsync("/api/v1/follow-up-rules", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateFollowUpRule_NonAdminRole_Returns403()
    {
        // Arrange
        var vetClient = CreateVetClient();
        var request = new CreateFollowUpRuleRequest("Dental", 7, "Dental follow-up");

        // Act
        var response = await vetClient.PostAsJsonAsync("/api/v1/follow-up-rules", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/follow-up-rules ───────────────────────────────────────

    [Fact]
    public async Task ListFollowUpRules_Authenticated_ReturnsSuccess()
    {
        // Arrange — create one first so the list is non-empty
        var adminClient = CreateAdminClient();
        var createRequest = new CreateFollowUpRuleRequest("Deworming", 90, "Quarterly deworming reminder");
        await adminClient.PostAsJsonAsync("/api/v1/follow-up-rules", createRequest, JsonOptions);

        // Act
        var response = await adminClient.GetAsync("/api/v1/follow-up-rules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<FollowUpRuleDto>>(JsonOptions);
        items.Should().NotBeNull();
        items!.Should().Contain(r => r.ConsultationType == "Deworming");
    }

    [Fact]
    public async Task ListFollowUpRules_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().GetAsync("/api/v1/follow-up-rules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/v1/follow-up-rules/{id} ──────────────────────────────────

    [Fact]
    public async Task UpdateFollowUpRule_ValidRequest_ReturnsSuccess()
    {
        // Arrange — create one first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateFollowUpRuleRequest("Checkup", 60, "Routine follow-up");
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/follow-up-rules", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<FollowUpRuleDto>(JsonOptions);

        var updateRequest = new UpdateFollowUpRuleRequest(
            ConsultationType: "Annual Checkup",
            FollowUpDays: 365,
            FollowUpReason: "Annual wellness exam");

        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/v1/follow-up-rules/{created!.Id}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<FollowUpRuleDto>(JsonOptions);
        updated.Should().NotBeNull();
        updated!.ConsultationType.Should().Be("Annual Checkup");
        updated.FollowUpDays.Should().Be(365);
    }

    [Fact]
    public async Task UpdateFollowUpRule_Unauthenticated_Returns401()
    {
        // Act
        var updateRequest = new UpdateFollowUpRuleRequest("Test", 7, "Test reason");
        var response = await Client.WithoutAuth().PutAsJsonAsync($"/api/v1/follow-up-rules/{Guid.NewGuid()}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/follow-up-rules/{id} ───────────────────────────────

    [Fact]
    public async Task DeactivateFollowUpRule_ValidId_ReturnsSuccess()
    {
        // Arrange — create one first
        var adminClient = CreateAdminClient();
        var createRequest = new CreateFollowUpRuleRequest("Spay Recovery", 10, "Post-spay check");
        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/follow-up-rules", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<FollowUpRuleDto>(JsonOptions);

        // Act
        var response = await adminClient.DeleteAsync($"/api/v1/follow-up-rules/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivateFollowUpRule_Unauthenticated_Returns401()
    {
        // Act
        var response = await Client.WithoutAuth().DeleteAsync($"/api/v1/follow-up-rules/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
