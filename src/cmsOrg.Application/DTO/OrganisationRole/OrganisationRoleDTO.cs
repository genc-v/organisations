namespace cmsOrg.Application.DTO;

public class OrganisationRoleDTO
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
}
