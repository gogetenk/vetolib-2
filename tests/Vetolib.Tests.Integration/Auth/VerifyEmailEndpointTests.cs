using System.Net;
using FluentAssertions;
using Vetolib.Tests.Integration.Infrastructure;

namespace Vetolib.Tests.Integration.Auth;

/// <summary>
/// Integration tests for POST /api/v1/auth/verify-email endpoint.
/// Wiring tests only — verifies HTTP contract (status code, auth).
/// </summary>
public sealed class VerifyEmailEndpointTests : IntegrationTestBase
{
    public VerifyEmailEndpointTests(VetolibWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task VerifyEmail_WithToken_ReturnsNon5xx()
    {
        // Arrange — use a dummy token; the handler will return an error result, not a 500
        var response = await Client.PostAsync("/api/v1/auth/verify-email?token=invalid-token-123", null);

        // Assert — should be a 4xx (invalid/not-found token), NOT 5xx (wiring broken)
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
    }
}
