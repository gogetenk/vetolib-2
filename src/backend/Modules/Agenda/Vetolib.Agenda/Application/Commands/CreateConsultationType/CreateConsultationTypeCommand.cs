using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.CreateConsultationType;

internal record CreateConsultationTypeCommand(
    Guid ClinicId,
    string Name,
    int DurationMinutes,
    int SortOrder = 0,
    bool RequiresVetSelection = false
) : IRequest<Result<ConsultationTypeDto>>;
