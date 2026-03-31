using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalVaccinations;

internal record GetAnimalVaccinationsQuery(Guid OwnerAccountId, Guid PatientId)
    : IRequest<Result<IReadOnlyList<PortalVaccinationDto>>>;
