namespace cmsOrg.Domain.Entities;

public class UserOrganisationPermission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid PermissionId { get; set; }

    public Organisation Organisation { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
