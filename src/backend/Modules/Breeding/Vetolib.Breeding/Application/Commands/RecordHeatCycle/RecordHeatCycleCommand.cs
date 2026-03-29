using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Commands.RecordHeatCycle;

internal record RecordHeatCycleCommand(
    Guid ClinicId,
    Guid PatientId,
    DateOnly StartDate,
    DateOnly? EndDate = null,
    string? Notes = null) : IRequest<Result<HeatCycleDto>>;
