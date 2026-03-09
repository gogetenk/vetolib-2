using MediatR;

namespace Vetolib.Shared.Kernel;

/// <summary>
/// Marker interface for domain events. Domain events are dispatched in-process
/// via MediatR after SaveChangesAsync, within the same transaction boundary.
/// </summary>
public interface IDomainEvent : INotification { }
