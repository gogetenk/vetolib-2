using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// Join entity between ClinicGroup and Clinic.
/// Cross-tenant by design — NOT IMultiTenant.
/// </summary>
internal class ClinicGroupMember : BaseEntity
{
    public Guid ClinicGroupId { get; private set; }
    public Guid ClinicId { get; private set; }

    private ClinicGroupMember() { } // EF Core constructor

    internal static ClinicGroupMember Create(Guid clinicGroupId, Guid clinicId)
    {
        return new ClinicGroupMember
        {
            ClinicGroupId = clinicGroupId,
            ClinicId = clinicId
        };
    }
}
