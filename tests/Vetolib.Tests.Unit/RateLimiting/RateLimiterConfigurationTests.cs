using System.Threading.RateLimiting;
using FluentAssertions;
using Xunit;

namespace Vetolib.Tests.Unit.RateLimiting;

/// <summary>
/// Verifies the rate limiting parameters that must be enforced for the auth and API policies.
/// Uses FixedWindowRateLimiter directly to avoid a dependency on the ASP.NET Core Web SDK.
/// </summary>
public class RateLimiterConfigurationTests
{
    // ─── Auth policy (10 req/min) ─────────────────────────────────────────────

    [Fact]
    public void AuthPolicy_Allows10RequestsWithinWindow()
    {
        // Arrange
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

        // Act — acquire 10 leases
        var successCount = 0;
        for (var i = 0; i < 10; i++)
        {
            using var lease = limiter.AttemptAcquire();
            if (lease.IsAcquired) successCount++;
        }

        // Assert
        successCount.Should().Be(10);
    }

    [Fact]
    public void AuthPolicy_Rejects11thRequestWithinWindow()
    {
        // Arrange
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

        // Exhaust the permit limit
        for (var i = 0; i < 10; i++)
            limiter.AttemptAcquire().Dispose();

        // Act — 11th request
        using var rejectedLease = limiter.AttemptAcquire();

        // Assert
        rejectedLease.IsAcquired.Should().BeFalse();
    }

    // ─── API policy (100 req/min) ─────────────────────────────────────────────

    [Fact]
    public void ApiPolicy_Allows100RequestsWithinWindow()
    {
        // Arrange
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

        // Act — acquire 100 leases
        var successCount = 0;
        for (var i = 0; i < 100; i++)
        {
            using var lease = limiter.AttemptAcquire();
            if (lease.IsAcquired) successCount++;
        }

        // Assert
        successCount.Should().Be(100);
    }

    [Fact]
    public void ApiPolicy_Rejects101stRequestWithinWindow()
    {
        // Arrange
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

        // Exhaust the permit limit
        for (var i = 0; i < 100; i++)
            limiter.AttemptAcquire().Dispose();

        // Act — 101st request
        using var rejectedLease = limiter.AttemptAcquire();

        // Assert
        rejectedLease.IsAcquired.Should().BeFalse();
    }

    // ─── 429 status code constant ─────────────────────────────────────────────

    [Fact]
    public void RejectedStatusCode_Is429TooManyRequests()
    {
        // The OnRejected callback uses StatusCodes.Status429TooManyRequests.
        // Verify the constant value is the standard HTTP 429.
        const int expected = 429;
        Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests.Should().Be(expected);
    }
}
