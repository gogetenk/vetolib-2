using Microsoft.AspNetCore.Http;

namespace Vetolib.Agenda.Infrastructure;

internal class AgendaPortalContext : IAgendaPortalContext
{
    private static readonly string OwnerIdKey = "AgendaPortal.OwnerId";
    private static readonly string ClinicIdKey = "AgendaPortal.ClinicId";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AgendaPortalContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid OwnerId =>
        _httpContextAccessor.HttpContext?.Items.TryGetValue(OwnerIdKey, out var v) == true && v is Guid g
            ? g
            : Guid.Empty;

    public Guid ClinicId =>
        _httpContextAccessor.HttpContext?.Items.TryGetValue(ClinicIdKey, out var v) == true && v is Guid g
            ? g
            : Guid.Empty;

    internal static void Populate(HttpContext context, Guid ownerId, Guid clinicId)
    {
        context.Items[OwnerIdKey] = ownerId;
        context.Items[ClinicIdKey] = clinicId;
    }
}
