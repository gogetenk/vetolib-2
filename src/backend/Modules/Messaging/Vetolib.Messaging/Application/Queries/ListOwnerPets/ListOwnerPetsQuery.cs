using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListOwnerPets;

internal record ListOwnerPetsQuery(Guid OwnerId, Guid ClinicId)
    : IRequest<Result<IReadOnlyList<PatientDto>>>;
