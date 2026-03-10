using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.BulkUpdatePreferences;

internal record PreferenceUpdateItem(PreferenceKey Key, string Value);

internal record BulkUpdatePreferencesCommand(
    Guid ClinicId,
    Guid UserId,
    IReadOnlyList<PreferenceUpdateItem> Preferences,
    string? IpAddress,
    string? UserAgent
) : IRequest<Result>;
