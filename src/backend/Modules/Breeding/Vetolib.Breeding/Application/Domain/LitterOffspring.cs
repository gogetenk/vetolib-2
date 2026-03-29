using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Application.Domain;

internal class LitterOffspring : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid LitterId { get; private set; }
    public Guid PatientId { get; private set; }
    public int? BirthOrder { get; private set; }

    private LitterOffspring() { } // EF Core constructor

    internal static LitterOffspring Create(Guid clinicId, Guid litterId, Guid patientId, int? birthOrder)
    {
        return new LitterOffspring
        {
            ClinicId = clinicId,
            LitterId = litterId,
            PatientId = patientId,
            BirthOrder = birthOrder
        };
    }
}
