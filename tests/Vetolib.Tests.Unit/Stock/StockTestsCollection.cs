using Xunit;

namespace Vetolib.Tests.Unit.Stock;

/// <summary>
/// All Stock test classes run sequentially within this collection to prevent EF Core
/// InMemory query-plan caching from interfering across parallel test class executions.
/// The global query filter uses Expression.Constant(clinicContext), and EF Core caches
/// the compiled query plan per DbContext type — parallel tests with different IClinicContext
/// instances can pick up a stale cached plan that uses a different ClinicId.
/// </summary>
[CollectionDefinition("StockTests")]
public class StockTestsCollection
{
}
