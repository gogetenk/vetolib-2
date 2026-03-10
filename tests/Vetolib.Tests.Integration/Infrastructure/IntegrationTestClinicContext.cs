using Vetolib.Shared.Kernel;

namespace Vetolib.Tests.Integration.Infrastructure;

/// <summary>
/// Test implementation of IClinicContext.
/// MUST use a fixed GUID (not Guid.NewGuid()) because EF Core compiles the multi-tenant
/// query filter once at model creation time and captures the ClinicId value as a constant.
/// </summary>
public sealed class IntegrationTestClinicContext : IClinicContext
{
    /// <summary>
    /// Primary test clinic — used as the default for all single-tenant tests.
    /// </summary>
    public static readonly Guid PrimaryClinicId = new Guid("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Secondary test clinic — used exclusively for tenant isolation tests.
    /// </summary>
    public static readonly Guid SecondaryClinicId = new Guid("22222222-2222-2222-2222-222222222222");

    public Guid ClinicId { get; set; } = PrimaryClinicId;
}
