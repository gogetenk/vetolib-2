using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.Portal.GetAnimalPrescriptions;

internal record GetAnimalPrescriptionsQuery(Guid OwnerAccountId, Guid PatientId)
    : IRequest<Result<IReadOnlyList<PortalPrescriptionDto>>>;
