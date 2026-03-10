using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Queries.GetUserPreferences;

internal record GetUserPreferencesQuery(Guid UserId) : IRequest<Result<List<PreferenceCategoryDto>>>;
