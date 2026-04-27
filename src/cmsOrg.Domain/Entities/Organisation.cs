namespace cmsOrg.Domain.Entities;

public class Organisation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserOrganisationPermission> UserPermissions { get; set; } = [];
public ICollection<OrganisationApiKey> ApiKeys { get; set; } = [];
}
