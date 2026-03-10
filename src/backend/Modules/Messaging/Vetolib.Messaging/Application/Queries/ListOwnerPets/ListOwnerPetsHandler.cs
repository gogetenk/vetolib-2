using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListOwnerPets;

internal class ListOwnerPetsHandler
    : IRequestHandler<ListOwnerPetsQuery, Result<IReadOnlyList<PatientDto>>>
{
    private readonly IPatientReader _patientReader;

    public ListOwnerPetsHandler(IPatientReader patientReader)
    {
        _patientReader = patientReader;
    }

    public async Task<Result<IReadOnlyList<PatientDto>>> Handle(
        ListOwnerPetsQuery request,
        CancellationToken cancellationToken)
    {
        return await _patientReader.GetPatientsByOwnerIdAsync(request.OwnerId, request.ClinicId, cancellationToken);
    }
}
