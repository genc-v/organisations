namespace cmsOrg.Application.DTO;

public class OrganisationAssetDTO
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
