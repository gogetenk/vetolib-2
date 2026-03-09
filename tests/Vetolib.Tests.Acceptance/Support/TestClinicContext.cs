using Vetolib.Shared.Kernel;

namespace Vetolib.Tests.Acceptance.Support;

internal class TestClinicContext : IClinicContext
{
    /// <summary>
    /// Fixed GUID used as the test clinic identifier.
    /// MUST be fixed (not random) because EF Core compiles the multi-tenant query filter
    /// once per model (at DbContext first use) and captures the ClinicId VALUE at that time.
    /// If this were Guid.NewGuid(), the filter would bake in a random value and subsequent
    /// changes to ClinicId would not affect queries. By using a fixed GUID, the baked-in
    /// value matches what we set in test steps. All scenarios share this clinic ID;
    /// AfterScenario cleanup uses IgnoreQueryFilters() to delete data between tests.
    /// </summary>
    public static readonly Guid TestClinicGuid = new Guid("11111111-1111-1111-1111-111111111111");

    public Guid ClinicId { get; set; } = TestClinicGuid;
}
