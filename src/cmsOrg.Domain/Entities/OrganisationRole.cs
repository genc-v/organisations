namespace cmsOrg.Domain.Entities;

public class OrganisationRole
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Organisation Organisation { get; set; } = null!;
    public ICollection<UserOrganisationRole> UserRoles { get; set; } = [];
}
