using Ardalis.Result;
using MediatR;

namespace Vetolib.Agenda.Application.Commands.CancelAppointmentSeries;

internal record CancelAppointmentSeriesCommand(Guid SeriesId) : IRequest<Result<int>>;
