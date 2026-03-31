using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalRecords;

internal record GetAnimalRecordsQuery(Guid OwnerAccountId, Guid PatientId)
    : IRequest<Result<IReadOnlyList<PortalMedicalRecordDto>>>;
