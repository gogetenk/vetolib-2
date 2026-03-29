using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.CreatePregnancy;

internal record CreatePregnancyCommand(
    Guid PatientId,
    Guid? FatherPatientId,
    DateOnly MatingDate,
    MatingMethod MatingMethod,
    string PatientSex,
    string PatientSpecies,
    string? Notes
) : IRequest<Result<PregnancyDto>>;
