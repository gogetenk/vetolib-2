using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Vetolib.Auth.Contracts;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for ClinicGroup endpoints (/api/v1/clinic-groups/*).
/// Wiring tests: verifies HTTP contract, auth, and RBAC — no business logic.
/// </summary>
public sealed class ClinicGroupEndpointsTests : IntegrationTestBase
{
    private static readonly Guid FakeGroupId = Guid.NewGuid();

    public ClinicGroupEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── POST /api/v1/clinic-groups ──────────────────────────────────────

    [Fact]
    public async Task CreateClinicGroup_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();
        var request = new CreateClinicGroupRequest("Al Barsha Group");

        var response = await client.PostAsJsonAsync("/api/v1/clinic-groups", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateClinicGroup_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();
        var request = new CreateClinicGroupRequest("Unauthorized Group");

        var response = await client.PostAsJsonAsync("/api/v1/clinic-groups", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateClinicGroup_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();
        var request = new CreateClinicGroupRequest("Receptionist Group");

        var response = await client.PostAsJsonAsync("/api/v1/clinic-groups", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /api/v1/clinic-groups/{id}/clinics ─────────────────────────

    [Fact]
    public async Task AddClinicToGroup_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();
        var request = new AddClinicToGroupRequest(Guid.NewGuid());

        var response = await client.PostAsJsonAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddClinicToGroup_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();
        var request = new AddClinicToGroupRequest(Guid.NewGuid());

        var response = await client.PostAsJsonAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddClinicToGroup_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();
        var request = new AddClinicToGroupRequest(Guid.NewGuid());

        var response = await client.PostAsJsonAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/clinic-groups/{id}/clinics ──────────────────────────

    [Fact]
    public async Task ListGroupClinics_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListGroupClinics_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DELETE /api/v1/clinic-groups/{id}/clinics/{clinicId} ─────────────

    [Fact]
    public async Task RemoveClinicFromGroup_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.DeleteAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveClinicFromGroup_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.DeleteAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveClinicFromGroup_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.DeleteAsync($"/api/v1/clinic-groups/{FakeGroupId}/clinics/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/clinic-groups/{id}/dashboard/stats ──────────────────

    [Fact]
    public async Task GetGroupDashboardStats_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/stats");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupDashboardStats_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupDashboardStats_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/clinic-groups/{id}/dashboard/clinics ────────────────

    [Fact]
    public async Task GetGroupClinicStats_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/clinics");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupClinicStats_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/clinics");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupClinicStats_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/clinics");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── GET /api/v1/clinic-groups/{id}/dashboard/revenue-comparison ─────

    [Fact]
    public async Task GetGroupRevenueComparison_AsAdmin_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/revenue-comparison");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGroupRevenueComparison_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/revenue-comparison");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGroupRevenueComparison_AsReceptionist_Returns403()
    {
        var client = CreateReceptionistClient();

        var response = await client.GetAsync($"/api/v1/clinic-groups/{FakeGroupId}/dashboard/revenue-comparison");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── POST /api/v1/auth/switch-clinic ─────────────────────────────────

    [Fact]
    public async Task SwitchClinic_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();
        var request = new SwitchClinicRequest(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/v1/auth/switch-clinic", request, JsonOptions);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SwitchClinic_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();
        var request = new SwitchClinicRequest(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/v1/auth/switch-clinic", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
