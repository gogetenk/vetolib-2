namespace Vetolib.Agenda.Infrastructure;

/// <summary>
/// Provides the authenticated owner identity for portal booking endpoints.
/// Populated by AgendaBookingPortalFilter after validating the magic link token.
/// </summary>
internal interface IAgendaPortalContext
{
    Guid OwnerId { get; }
    Guid ClinicId { get; }
}
