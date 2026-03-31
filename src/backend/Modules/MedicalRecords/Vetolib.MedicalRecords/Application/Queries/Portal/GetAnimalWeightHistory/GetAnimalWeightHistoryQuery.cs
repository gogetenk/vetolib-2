using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalWeightHistory;

internal record GetAnimalWeightHistoryQuery(Guid OwnerAccountId, Guid PatientId)
    : IRequest<Result<IReadOnlyList<PortalWeightEntryDto>>>;
