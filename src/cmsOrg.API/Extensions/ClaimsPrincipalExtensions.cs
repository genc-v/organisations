using System.Security.Claims;

namespace cmsOrg.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
