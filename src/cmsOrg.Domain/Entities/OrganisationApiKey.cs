namespace cmsOrg.Domain.Entities;

public class OrganisationApiKey
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Organisation Organisation { get; set; } = null!;
}
