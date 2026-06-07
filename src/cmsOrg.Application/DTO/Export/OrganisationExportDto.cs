namespace cmsOrg.Application.DTO.Export;

public class OrganisationExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}
