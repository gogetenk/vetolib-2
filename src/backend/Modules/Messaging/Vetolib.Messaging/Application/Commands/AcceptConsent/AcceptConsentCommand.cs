using Ardalis.Result;
using MediatR;

namespace Vetolib.Messaging.Application.Commands.AcceptConsent;

internal record AcceptConsentCommand(Guid OwnerId, Guid ClinicId, string ConsentVersion)
    : IRequest<Result>;
