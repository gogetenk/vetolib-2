using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Queries.GetUserPreferencesByCategory;

internal record GetUserPreferencesByCategoryQuery(
    Guid UserId,
    PreferenceCategory Category
) : IRequest<Result<List<PreferenceDto>>>;
