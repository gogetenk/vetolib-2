using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Queries.GetWorkingHours;

internal record GetWorkingHoursQuery : IRequest<Result<IReadOnlyList<WorkingHoursDto>>>;
