using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetPregnanciesByPatient;

internal record GetPregnanciesByPatientQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<PregnancyDto>>>;
