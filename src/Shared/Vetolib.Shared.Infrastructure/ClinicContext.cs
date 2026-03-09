using Microsoft.AspNetCore.Http;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

public class ClinicContext : IClinicContext
{
    private readonly IHttpContextAccessor _accessor;

    public ClinicContext(IHttpContextAccessor accessor)
        => _accessor = accessor;

    public Guid ClinicId
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst("clinic_id");
            if (claim is null || !Guid.TryParse(claim.Value, out var id))
                return Guid.Empty;
            return id;
        }
    }
}
