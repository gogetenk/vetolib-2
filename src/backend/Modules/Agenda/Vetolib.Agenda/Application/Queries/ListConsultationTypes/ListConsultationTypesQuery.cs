using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListConsultationTypes;

internal record ListConsultationTypesQuery() : IRequest<Result<IReadOnlyList<ConsultationTypeDto>>>;
