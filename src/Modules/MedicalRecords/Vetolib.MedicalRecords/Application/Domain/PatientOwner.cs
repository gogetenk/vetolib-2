using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class PatientOwner : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; }
    public Guid OwnerId { get; private set; }
    public Owner? Owner { get; private set; }

    private PatientOwner() { } // EF Core constructor

    public static PatientOwner Create(Guid clinicId, Guid patientId, Guid ownerId)
    {
        return new PatientOwner
        {
            ClinicId = clinicId,
            PatientId = patientId,
            OwnerId = ownerId
        };
    }
}
