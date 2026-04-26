namespace cmsOrg.Domain.Entities;

public enum OrganisationRole
{
    Viewer = 1,
    Editor = 2,
    Admin = 3
}

public class UserOrganisationRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public OrganisationRole Role { get; set; }

    public Organisation Organisation { get; set; } = null!;
}
