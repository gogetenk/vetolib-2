using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetMyAnimals;

internal record GetMyAnimalsQuery(Guid OwnerAccountId)
    : IRequest<Result<IReadOnlyList<PortalAnimalDto>>>;
