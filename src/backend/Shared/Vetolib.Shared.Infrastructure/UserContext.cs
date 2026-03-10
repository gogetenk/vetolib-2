using Microsoft.AspNetCore.Http;
using Vetolib.Shared.Kernel;

namespace Vetolib.Shared.Infrastructure;

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public UserContext(IHttpContextAccessor accessor)
        => _accessor = accessor;

    public string? UserEmail
    {
        get
        {
            var http = _accessor.HttpContext;
            if (http is null) return null;

            // Try the "email" claim first, then the standard "name" claim.
            return http.User.FindFirst("email")?.Value
                ?? http.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? http.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        }
    }
}
