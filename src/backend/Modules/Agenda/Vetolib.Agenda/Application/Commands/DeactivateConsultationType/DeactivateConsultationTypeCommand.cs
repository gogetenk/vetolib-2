using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Application.Commands.DeactivateConsultationType;

internal record DeactivateConsultationTypeCommand(Guid Id) : IRequest<Result>;
