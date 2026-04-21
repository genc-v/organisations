namespace cmsOrg.Domain.Entities;

public class UserOrganisationRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid RoleId { get; set; }

    public Organisation Organisation { get; set; } = null!;
    public OrganisationRole Role { get; set; } = null!;
}
