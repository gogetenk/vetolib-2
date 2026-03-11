using Ardalis.Result;
using MediatR;
using Vetolib.Agenda.Contracts;

namespace Vetolib.Agenda.Application.Queries.ListPublicConsultationTypes;

/// <summary>
/// Returns active consultation types for a given clinic.
/// Used by the public (no-auth) booking portal endpoint.
/// ClinicId is passed explicitly because there is no authenticated tenant context.
/// </summary>
internal record ListPublicConsultationTypesQuery(Guid ClinicId)
    : IRequest<Result<List<ConsultationTypeDto>>>;
