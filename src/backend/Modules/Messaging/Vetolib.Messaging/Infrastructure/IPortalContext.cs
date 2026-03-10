namespace Vetolib.Messaging.Infrastructure;

internal interface IPortalContext
{
    Guid OwnerId { get; }
    Guid ClinicId { get; }
}
