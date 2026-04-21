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

public class OrganisationAssetService(AppDbContext db, MongoDbContext mongo, IHttpContextAccessor http, IDistributedCache cache) : IOrganisationAssetService
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
            ResourceType = "OrganisationAsset",
            ResourceId = resourceId
        });

    public async Task<PaginatedResult<OrganisationAssetDTO>> GetByOrganisation(Guid organisationId, int page, int pageSize)
    {
        var query = db.OrganisationAssets.Where(a => a.OrganisationId == organisationId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new OrganisationAssetDTO
            {
                Id = a.Id,
                OrganisationId = a.OrganisationId,
                Name = a.Name,
                Type = a.Type,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        WriteLog("GetOrganisationAssets", organisationId.ToString());
        return new PaginatedResult<OrganisationAssetDTO> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize };
    }

    public async Task<OrganisationAssetDTO> GetById(Guid id)
    {
        var cacheKey = $"asset:{id}";
        var cached = await cache.GetStringAsync(cacheKey);
        if (cached is not null)
            return JsonSerializer.Deserialize<OrganisationAssetDTO>(cached)!;

        var asset = await db.OrganisationAssets.FindAsync(id)
            ?? throw AppException.NotFound("Asset not found.");

        var dto = new OrganisationAssetDTO { Id = asset.Id, OrganisationId = asset.OrganisationId, Name = asset.Name, Type = asset.Type, CreatedAt = asset.CreatedAt };
        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        WriteLog("GetOrganisationAsset", asset.OrganisationId.ToString(), id.ToString());
        return dto;
    }

    public async Task<OrganisationAssetDTO> Create(Guid organisationId, CreateOrganisationAssetDTO dto)
    {
        if (!await db.Organisations.AnyAsync(o => o.Id == organisationId))
            throw AppException.NotFound("Organisation not found.");

        var asset = new OrganisationAsset { Id = Guid.NewGuid(), OrganisationId = organisationId, Name = dto.Name, Type = dto.Type };
        db.OrganisationAssets.Add(asset);
        await db.SaveChangesAsync();
        WriteLog("CreateOrganisationAsset", organisationId.ToString(), asset.Id.ToString());

        return new OrganisationAssetDTO { Id = asset.Id, OrganisationId = asset.OrganisationId, Name = asset.Name, Type = asset.Type, CreatedAt = asset.CreatedAt };
    }

    public async Task Update(Guid id, UpdateOrganisationAssetDTO dto)
    {
        var asset = await db.OrganisationAssets.FindAsync(id)
            ?? throw AppException.NotFound("Asset not found.");

        asset.Name = dto.Name;
        asset.Type = dto.Type;
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"asset:{id}");
        WriteLog("UpdateOrganisationAsset", asset.OrganisationId.ToString(), id.ToString());
    }

    public async Task Delete(Guid id)
    {
        var asset = await db.OrganisationAssets.FindAsync(id)
            ?? throw AppException.NotFound("Asset not found.");

        var organisationId = asset.OrganisationId.ToString();
        db.OrganisationAssets.Remove(asset);
        await db.SaveChangesAsync();
        await cache.RemoveAsync($"asset:{id}");
        WriteLog("DeleteOrganisationAsset", organisationId, id.ToString());
    }
}
