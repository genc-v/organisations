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
            ResourceType = "UserOrganisationRole",
            ResourceId = resourceId
        });

    public async Task<List<UserOrganisationRoleDTO>> GetByOrganisation(Guid organisationId)
    {
        var result = await db.UserOrganisationRoles
            .Where(u => u.OrganisationId == organisationId)
            .Include(u => u.Role)
            .Select(u => new UserOrganisationRoleDTO
            {
                Id = u.Id,
                UserId = u.UserId,
                OrganisationId = u.OrganisationId,
                RoleId = u.RoleId,
                RoleName = u.Role.Name
            })
            .ToListAsync();

        WriteLog("GetOrganisationMembers", organisationId.ToString());
        return result;
    }

    public async Task<UserOrganisationRoleDTO> Assign(AssignUserRoleDTO dto)
    {
        if (!await db.Organisations.AnyAsync(o => o.Id == dto.OrganisationId))
            throw AppException.NotFound("Organisation not found.");

        if (!await db.OrganisationRoles.AnyAsync(r => r.Id == dto.RoleId && r.OrganisationId == dto.OrganisationId))
            throw AppException.NotFound("Role not found in this organisation.");

        if (await db.UserOrganisationRoles.AnyAsync(u => u.UserId == dto.UserId && u.OrganisationId == dto.OrganisationId && u.RoleId == dto.RoleId))
            throw AppException.Conflict("User already has this role in the organisation.");

        var entry = new UserOrganisationRole
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            OrganisationId = dto.OrganisationId,
            RoleId = dto.RoleId
        };
        db.UserOrganisationRoles.Add(entry);
        await db.SaveChangesAsync();
        WriteLog("AssignUserRole", dto.OrganisationId.ToString(), entry.Id.ToString());

        var role = await db.OrganisationRoles.FindAsync(dto.RoleId);
        return new UserOrganisationRoleDTO
        {
            Id = entry.Id,
            UserId = entry.UserId,
            OrganisationId = entry.OrganisationId,
            RoleId = entry.RoleId,
            RoleName = role!.Name
        };
    }

    public async Task Remove(Guid id)
    {
        var entry = await db.UserOrganisationRoles.FindAsync(id)
            ?? throw AppException.NotFound("User organisation role not found.");

        var organisationId = entry.OrganisationId.ToString();
        db.UserOrganisationRoles.Remove(entry);
        await db.SaveChangesAsync();
        WriteLog("RemoveUserRole", organisationId, id.ToString());
    }
}
