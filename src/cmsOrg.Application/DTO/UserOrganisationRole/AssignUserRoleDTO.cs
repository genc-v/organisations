using System.Text.Json.Serialization;

namespace cmsOrg.Application.DTO;

public class AssignUserRoleDTO
{
    public Guid UserId { get; set; }
    public string RoleTemplate { get; set; } = "Viewer";
    [JsonIgnore] public Guid OrganisationId { get; set; }
}
