using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests that verify MassTransit event publishing/consuming.
/// Extends IntegrationTestBase to add ITestHarness access.
/// </summary>
[Collection("Integration")]
public abstract class EventIntegrationTestBase : IntegrationTestBase
{
    protected readonly ITestHarness Harness;

    protected EventIntegrationTestBase(VetolibWebApplicationFactory factory) : base(factory)
    {
        Harness = factory.Services.GetRequiredService<ITestHarness>();
    }

    public override async Task InitializeAsync()
    {
        await Harness.Start();
    }

    public override async Task DisposeAsync()
    {
        await Harness.Stop();
        await base.DisposeAsync();
    }
}
