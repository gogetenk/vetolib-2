using Microsoft.AspNetCore.Http;

namespace Vetolib.Messaging.Infrastructure;

internal class PortalContext : IPortalContext
{
    private const string OwnerIdKey = "portal_owner_id";
    private const string ClinicIdKey = "portal_clinic_id";

    private readonly IHttpContextAccessor _accessor;

    public PortalContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid OwnerId
    {
        get
        {
            var items = _accessor.HttpContext?.Items;
            if (items is not null && items.TryGetValue(OwnerIdKey, out var value) && value is Guid id)
                return id;
            return Guid.Empty;
        }
    }

    public Guid ClinicId
    {
        get
        {
            var items = _accessor.HttpContext?.Items;
            if (items is not null && items.TryGetValue(ClinicIdKey, out var value) && value is Guid id)
                return id;
            return Guid.Empty;
        }
    }

    public static void Populate(HttpContext context, Guid ownerId, Guid clinicId)
    {
        context.Items[OwnerIdKey] = ownerId;
        context.Items[ClinicIdKey] = clinicId;
    }
}
