namespace cmsOrg.Domain.Entities;

public class Organisation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrganisationRole> Roles { get; set; } = [];
    public ICollection<UserOrganisationRole> UserRoles { get; set; } = [];
    public ICollection<OrganisationAsset> Assets { get; set; } = [];
    public ICollection<OrganisationApiKey> ApiKeys { get; set; } = [];
}
