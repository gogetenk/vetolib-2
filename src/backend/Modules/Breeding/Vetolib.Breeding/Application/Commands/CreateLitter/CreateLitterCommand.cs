using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.CreateLitter;

internal record CreateLitterCommand(
    Guid ClinicId,
    Guid MotherPatientId,
    Guid? FatherPatientId,
    string? ExternalFatherName,
    DateOnly BirthDate,
    int BornCount,
    int AliveCount,
    string? Notes) : IRequest<Result<LitterDto>>;
