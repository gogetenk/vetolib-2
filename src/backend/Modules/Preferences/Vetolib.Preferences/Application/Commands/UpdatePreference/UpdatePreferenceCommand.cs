using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.UpdatePreference;

internal record UpdatePreferenceCommand(
    Guid ClinicId,
    Guid UserId,
    PreferenceKey Key,
    string Value,
    string? IpAddress,
    string? UserAgent
) : IRequest<Result>;
