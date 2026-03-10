using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Commands.RevokeConsent;

internal record RevokeConsentCommand(
    Guid ClinicId,
    Guid UserId,
    PreferenceCategory Category,
    string? IpAddress,
    string? UserAgent
) : IRequest<Result>;
