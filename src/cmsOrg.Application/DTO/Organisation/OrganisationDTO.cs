namespace cmsOrg.Application.DTO;

public class OrganisationDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
