using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain.Events;

internal record EmailVerificationRequestedDomainEvent(
    Guid UserId,
    string Email,
    string VerificationToken) : IDomainEvent;
