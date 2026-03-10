using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.UpdateClinicDefaults;

internal record ClinicDefaultItem(PreferenceKey Key, string Value);

internal record UpdateClinicDefaultsCommand(
    Guid ClinicId,
    Guid AdminUserId,
    IReadOnlyList<ClinicDefaultItem> Defaults
) : IRequest<Result>;
