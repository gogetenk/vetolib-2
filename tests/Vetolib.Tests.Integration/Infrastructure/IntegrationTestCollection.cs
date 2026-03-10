namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// xUnit collection fixture that shares a single VetolibWebApplicationFactory
/// (and its Testcontainer) across all test classes in the "Integration" collection.
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<VetolibWebApplicationFactory>
{
}
