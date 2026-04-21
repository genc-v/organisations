namespace cmsOrg.Application.DTO;

public class AssignUserRoleDTO
{
    public required Guid UserId { get; set; }
    public required Guid OrganisationId { get; set; }
    public required Guid RoleId { get; set; }
}
