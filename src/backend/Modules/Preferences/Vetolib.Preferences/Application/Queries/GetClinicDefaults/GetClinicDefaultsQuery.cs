using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Queries.GetClinicDefaults;

internal record GetClinicDefaultsQuery : IRequest<Result<List<PreferenceCategoryDto>>>;
