using System.Security.Claims;
using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using cmsOrg.Domain.Entities;
using cmsOrg.Infrastructure.Persistence;
using cmsOrg.Infrastructure.Persistence.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace cmsOrg.Infrastructure.Services;

public class UserOrganisationRoleService(AppDbContext db, MongoDbContext mongo, IHttpContextAccessor http) : IUserOrganisationRoleService
{
    private string UserId => http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? http.HttpContext?.User.FindFirst("sub")?.Value
                          ?? string.Empty;

    private void WriteLog(string action, string organisationId, string? resourceId = null) =>
        _ = mongo.Logs.InsertOneAsync(new Log
        {
            UserId = UserId,
            OrganisationId = organisationId,
            Action = action,
            ResourceType = "UserOrganisationPermission",
            ResourceId = resourceId
        });

    public async Task<List<UserOrganisationRoleDTO>> GetByOrganisation(Guid organisationId)
    {
        var result = await db.UserOrganisationPermissions
            .Where(u => u.OrganisationId == organisationId)
            .Include(u => u.Permission)
            .Select(u => new UserOrganisationRoleDTO
            {
                Id = u.Id,
                UserId = u.UserId,
                OrganisationId = u.OrganisationId,
                Role = u.Permission.Name
            })
            .ToListAsync();

        WriteLog("GetOrganisationMembers", organisationId.ToString());
        return result;
    }

    public async Task<UserOrganisationRoleDTO> Assign(AssignUserRoleDTO dto)
    {
        if (!await db.Organisations.AnyAsync(o => o.Id == dto.OrganisationId))
            throw AppException.NotFound("Organisation not found.");

        if (await db.UserOrganisationPermissions.AnyAsync(u => u.UserId == dto.UserId && u.OrganisationId == dto.OrganisationId))
            throw AppException.Conflict("User already has a role in this organisation.");

        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == dto.RoleTemplate)
            ?? await db.Permissions.FirstAsync(p => p.Name == "Viewer");

        var entry = new UserOrganisationPermission
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            OrganisationId = dto.OrganisationId,
            PermissionId = permission.Id
        };
        db.UserOrganisationPermissions.Add(entry);
        await db.SaveChangesAsync();
        WriteLog("AssignUserRole", dto.OrganisationId.ToString(), entry.Id.ToString());

        return new UserOrganisationRoleDTO
        {
            Id = entry.Id,
            UserId = entry.UserId,
            OrganisationId = entry.OrganisationId,
            Role = permission.Name
        };
    }

    public async Task Remove(Guid organisationId, Guid id)
    {
        var entry = await db.UserOrganisationPermissions
            .FirstOrDefaultAsync(u => u.UserId == id && u.OrganisationId == organisationId)
            ?? throw AppException.NotFound("User organisation permission not found.");

        db.UserOrganisationPermissions.Remove(entry);
        await db.SaveChangesAsync();
        WriteLog("RemoveUserRole", organisationId.ToString(), id.ToString());
    }

    public async Task UpdateRole(Guid organisationId, Guid id, string roleName)
    {
        var entry = await db.UserOrganisationPermissions
            .FirstOrDefaultAsync(u => u.UserId == id && u.OrganisationId == organisationId)
            ?? throw AppException.NotFound("User organisation permission not found.");

        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == roleName)
            ?? throw AppException.NotFound($"Role '{roleName}' not found.");

        entry.PermissionId = permission.Id;
        await db.SaveChangesAsync();
        WriteLog("UpdateUserRole", organisationId.ToString(), id.ToString());
    }
}
