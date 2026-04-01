using System.Net;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for Onboarding endpoints (/api/v1/onboarding/*).
/// Wiring tests: verifies HTTP contract and auth — no business logic.
/// </summary>
public sealed class OnboardingEndpointsTests : IntegrationTestBase
{
    public OnboardingEndpointsTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    // ── GET /api/v1/onboarding ──────────────────────────────────────────

    [Fact]
    public async Task GetOnboardingState_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.GetAsync("/api/v1/onboarding");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOnboardingState_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.GetAsync("/api/v1/onboarding");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/onboarding/steps/{stepId}/complete ─────────────────

    [Fact]
    public async Task CompleteOnboardingStep_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.PostAsync("/api/v1/onboarding/steps/create-first-patient/complete", null);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteOnboardingStep_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.PostAsync("/api/v1/onboarding/steps/create-first-patient/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/onboarding/banner/dismiss ──────────────────────────

    [Fact]
    public async Task DismissWelcomeBanner_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.PostAsync("/api/v1/onboarding/banner/dismiss", null);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DismissWelcomeBanner_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.PostAsync("/api/v1/onboarding/banner/dismiss", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── POST /api/v1/onboarding/checklist/dismiss ───────────────────────

    [Fact]
    public async Task DismissChecklist_Authenticated_ReturnsNon5xx()
    {
        var client = CreateAdminClient();

        var response = await client.PostAsync("/api/v1/onboarding/checklist/dismiss", null);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DismissChecklist_Unauthenticated_Returns401()
    {
        var client = Factory.CreateClient().WithoutAuth();

        var response = await client.PostAsync("/api/v1/onboarding/checklist/dismiss", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
