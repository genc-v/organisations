namespace cmsOrg.Application.DTO;

public class UserOrganisationRoleDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganisationId { get; set; }
    public string Role { get; set; } = string.Empty;
}
