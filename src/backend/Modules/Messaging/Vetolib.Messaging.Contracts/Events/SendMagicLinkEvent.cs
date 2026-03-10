namespace Vetolib.Messaging.Contracts.Events;

public record SendMagicLinkEvent(
    Guid OwnerId,
    Guid ClinicId,
    string OwnerEmail,
    string PortalUrl);
