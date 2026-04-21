namespace cmsOrg.Application.DTO;

public class OrganisationApiKeyDTO
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}
