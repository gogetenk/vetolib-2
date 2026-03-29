using Ardalis.Result;
using MediatR;
using Vetolib.Breeding.Contracts;

namespace Vetolib.Breeding.Application.Queries.GetActivePregnancies;

internal record GetActivePregnanciesQuery() : IRequest<Result<IReadOnlyList<PregnancyDto>>>;
