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

public class OrganisationService(AppDbContext db, MongoDbContext mongo, IHttpContextAccessor http, IDistributedCache cache) : IOrganisationService
{
    private string UserId => http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? http.HttpContext?.User.FindFirst("sub")?.Value
                          ?? string.Empty;

    private void WriteLog(string action, string? resourceId = null, string? organisationId = null) =>
        _ = mongo.Logs.InsertOneAsync(new Log
        {
            UserId = UserId,
            OrganisationId = organisationId,
            Action = action,
            ResourceType = "Organisation",
            ResourceId = resourceId
        });

    public async Task<PaginatedResult<OrganisationDTO>> GetAll(int page, int pageSize, string? search)
    {
        var query = db.Organisations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.Name.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrganisationDTO { Id = o.Id, Name = o.Name, CreatedAt = o.CreatedAt })
            .ToListAsync();

        WriteLog("GetAllOrganisations");
        return new PaginatedResult<OrganisationDTO> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize };
    }

    public async Task<OrganisationDTO> GetById(Guid id)
    {
        var cacheKey = $"organisation:{id}";
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
            return JsonSerializer.Deserialize<OrganisationDTO>(cached)!;

        var org = await db.Organisations.FindAsync(id)
            ?? throw AppException.NotFound("Organisation not found.");

        var dto = new OrganisationDTO { Id = org.Id, Name = org.Name, CreatedAt = org.CreatedAt };
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        WriteLog("GetOrganisation", id.ToString(), id.ToString());
        return dto;
    }

    public async Task<OrganisationDTO> Create(CreateOrganisationDTO dto)
    {
        if (await db.Organisations.AnyAsync(o => o.Name == dto.Name))
            throw AppException.Conflict($"Organisation '{dto.Name}' already exists.");

        var org = new Organisation { Id = Guid.NewGuid(), Name = dto.Name };
        db.Organisations.Add(org);
        await db.SaveChangesAsync();
        WriteLog("CreateOrganisation", org.Id.ToString(), org.Id.ToString());

        return new OrganisationDTO { Id = org.Id, Name = org.Name, CreatedAt = org.CreatedAt };
    }

    public async Task Update(Guid id, UpdateOrganisationDTO dto)
    {
        var org = await db.Organisations.FindAsync(id)
            ?? throw AppException.NotFound("Organisation not found.");

        org.Name = dto.Name;
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"organisation:{id}");
        WriteLog("UpdateOrganisation", id.ToString(), id.ToString());
    }

    public async Task Delete(Guid id)
    {
        var org = await db.Organisations.FindAsync(id)
            ?? throw AppException.NotFound("Organisation not found.");

        db.Organisations.Remove(org);
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"organisation:{id}");
        WriteLog("DeleteOrganisation", id.ToString(), id.ToString());
    }
}
