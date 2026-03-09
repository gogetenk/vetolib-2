using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.CreateOwner;

internal record CreateOwnerCommand(
    Guid ClinicId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone) : IRequest<Result<OwnerDto>>;
