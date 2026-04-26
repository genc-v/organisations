using System.Text.Json.Serialization;

namespace cmsOrg.Application.DTO;

public class AssignUserRoleDTO
{
    public Guid UserId { get; set; }
    public string RoleTemplate { get; set; } = "Viewer"; // Viewer, Editor, Admin
    [JsonIgnore] public Guid OrganisationId { get; set; }
}
