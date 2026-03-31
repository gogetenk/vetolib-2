using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.CreateShareLink;

internal record CreateShareLinkCommand(
    Guid ClinicId,
    Guid PatientId,
    Guid OwnerAccountId) : IRequest<Result<CreateShareLinkResponse>>;
