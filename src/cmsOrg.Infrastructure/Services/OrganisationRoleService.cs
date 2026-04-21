using System.Security.Claims;
using System.Text.Json;
using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using cmsOrg.Domain.Entities;
using cmsOrg.Infrastructure.Persistence;
using cmsOrg.Infrastructure.Persistence.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace cmsOrg.Infrastructure.Services;

public class OrganisationRoleService(AppDbContext db, MongoDbContext mongo, IHttpContextAccessor http, IDistributedCache cache) : IOrganisationRoleService
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
            ResourceType = "OrganisationRole",
            ResourceId = resourceId
        });

    public async Task<PaginatedResult<OrganisationRoleDTO>> GetByOrganisation(Guid organisationId, int page, int pageSize)
    {
        var query = db.OrganisationRoles.Where(r => r.OrganisationId == organisationId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new OrganisationRoleDTO { Id = r.Id, OrganisationId = r.OrganisationId, Name = r.Name })
            .ToListAsync();

        WriteLog("GetOrganisationRoles", organisationId.ToString());
        return new PaginatedResult<OrganisationRoleDTO> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize };
    }

    public async Task<OrganisationRoleDTO> GetById(Guid id)
    {
        var cacheKey = $"role:{id}";
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
            return JsonSerializer.Deserialize<OrganisationRoleDTO>(cached)!;

        var role = await db.OrganisationRoles.FindAsync(id)
            ?? throw AppException.NotFound("Organisation role not found.");

        var dto = new OrganisationRoleDTO { Id = role.Id, OrganisationId = role.OrganisationId, Name = role.Name };
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        WriteLog("GetOrganisationRole", role.OrganisationId.ToString(), id.ToString());
        return dto;
    }

    public async Task<OrganisationRoleDTO> Create(Guid organisationId, CreateOrganisationRoleDTO dto)
    {
        if (!await db.Organisations.AnyAsync(o => o.Id == organisationId))
            throw AppException.NotFound("Organisation not found.");

        if (await db.OrganisationRoles.AnyAsync(r => r.OrganisationId == organisationId && r.Name == dto.Name))
            throw AppException.Conflict($"Role '{dto.Name}' already exists in this organisation.");

        var role = new OrganisationRole { Id = Guid.NewGuid(), OrganisationId = organisationId, Name = dto.Name };
        db.OrganisationRoles.Add(role);
        await db.SaveChangesAsync();
        WriteLog("CreateOrganisationRole", organisationId.ToString(), role.Id.ToString());

        return new OrganisationRoleDTO { Id = role.Id, OrganisationId = role.OrganisationId, Name = role.Name };
    }

    public async Task Update(Guid id, UpdateOrganisationRoleDTO dto)
    {
        var role = await db.OrganisationRoles.FindAsync(id)
            ?? throw AppException.NotFound("Organisation role not found.");

        role.Name = dto.Name;
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"role:{id}");
        WriteLog("UpdateOrganisationRole", role.OrganisationId.ToString(), id.ToString());
    }

    public async Task Delete(Guid id)
    {
        var role = await db.OrganisationRoles.FindAsync(id)
            ?? throw AppException.NotFound("Organisation role not found.");

        var organisationId = role.OrganisationId.ToString();
        db.OrganisationRoles.Remove(role);
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"role:{id}");
        WriteLog("DeleteOrganisationRole", organisationId, id.ToString());
    }
}
