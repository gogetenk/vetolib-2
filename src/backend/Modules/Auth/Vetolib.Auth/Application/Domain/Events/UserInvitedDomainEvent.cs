using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain.Events;

internal record UserInvitedDomainEvent(
    Guid UserId,
    string Email,
    string FullName,
    string TemporaryPassword,
    string ClinicName) : IDomainEvent;
