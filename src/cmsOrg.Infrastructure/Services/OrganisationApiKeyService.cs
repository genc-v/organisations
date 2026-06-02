using System.Security.Claims;
using System.Security.Cryptography;
using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using cmsOrg.Domain.Entities;
using cmsOrg.Infrastructure.Persistence;
using cmsOrg.Infrastructure.Persistence.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace cmsOrg.Infrastructure.Services;

public class OrganisationApiKeyService(AppDbContext db, MongoDbContext mongo, IHttpContextAccessor http) : IOrganisationApiKeyService
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
            ResourceType = "OrganisationApiKey",
            ResourceId = resourceId
        });

    public async Task<ValidateApiKeyResultDTO> Validate(string key)
    {
        var apiKey = await db.OrganisationApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == key && k.IsActive
                && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            ?? throw AppException.Unauthorized("Invalid API key.");

        return new ValidateApiKeyResultDTO { OrganisationId = apiKey.OrganisationId };
    }

    public async Task<List<OrganisationApiKeyDTO>> GetByOrganisation(Guid organisationId)
    {
        var result = await db.OrganisationApiKeys
            .Where(k => k.OrganisationId == organisationId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new OrganisationApiKeyDTO
            {
                Id = k.Id,
                OrganisationId = k.OrganisationId,
                Key = k.Key,
                CreatedAt = k.CreatedAt,
                ExpiresAt = k.ExpiresAt,
                IsActive = k.IsActive
            })
            .ToListAsync();

        WriteLog("GetOrganisationApiKeys", organisationId.ToString());
        return result;
    }

    public async Task<OrganisationApiKeyDTO> Create(Guid organisationId, CreateOrganisationApiKeyDTO dto)
    {

        var key = new OrganisationApiKey
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisationId,
            Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            ExpiresAt = dto.ExpiresAt,
            IsActive = true
        };
        db.OrganisationApiKeys.Add(key);
        await db.SaveChangesAsync();
        WriteLog("CreateOrganisationApiKey", organisationId.ToString(), key.Id.ToString());

        return new OrganisationApiKeyDTO { Id = key.Id, OrganisationId = key.OrganisationId, Key = key.Key, CreatedAt = key.CreatedAt, ExpiresAt = key.ExpiresAt, IsActive = key.IsActive };
    }

    public async Task Toggle(Guid organisationId, Guid id)
    {
        var key = await db.OrganisationApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.OrganisationId == organisationId)
            ?? throw AppException.NotFound("API key not found.");

        key.IsActive = !key.IsActive;
        await db.SaveChangesAsync();
        WriteLog(key.IsActive ? "ActivateApiKey" : "DeactivateApiKey", organisationId.ToString(), id.ToString());
    }

    public async Task Delete(Guid organisationId, Guid id)
    {
        var key = await db.OrganisationApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.OrganisationId == organisationId)
            ?? throw AppException.NotFound("API key not found.");

        db.OrganisationApiKeys.Remove(key);
        await db.SaveChangesAsync();
        WriteLog("DeleteOrganisationApiKey", organisationId.ToString(), id.ToString());
    }
}
