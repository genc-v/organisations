using cmsOrg.Application.Common;
using cmsOrg.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace cmsOrg.Infrastructure.Services;

public interface IAccessControlService
{
    Task CheckAccess(Guid userId, Guid organisationId, string roleRequired);
}

public class AccessControlService(AppDbContext db) : IAccessControlService
{
    public async Task CheckAccess(Guid userId, Guid organisationId, string roleRequired)
    {
        var userRole = await db.UserOrganisationPermissions
            .Include(uop => uop.Permission)
            .Where(uop => uop.UserId == userId && uop.OrganisationId == organisationId)
            .Select(uop => uop.Permission.Name)
            .FirstOrDefaultAsync();

        if (userRole == null)
        {
            throw AppException.Forbidden("User does not have access to this organisation.");
        }

        // Hierarchy logic: 
        // Admin (3) > Editor (2) > Viewer (1)
        int userWeight = GetRoleWeight(userRole);
        int requiredWeight = GetRoleWeight(roleRequired);

        if (userWeight < requiredWeight)
        {
            throw AppException.Forbidden($"User has role '{userRole}' but '{roleRequired}' is required.");
        }
    }

    private static int GetRoleWeight(string role) => role switch
    {
        "Admin" => 3,
        "Editor" => 2,
        "Viewer" => 1,
        _ => 0
    };
}
