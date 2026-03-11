using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Commands.UpdateConsultationType;

internal record UpdateConsultationTypeCommand(
    Guid Id,
    string Name,
    int DurationMinutes,
    int SortOrder,
    bool RequiresVetSelection
) : IRequest<Result<ConsultationTypeDto>>;
