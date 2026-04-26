using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IUserOrganisationRoleService
{
    Task<List<UserOrganisationRoleDTO>> GetByOrganisation(Guid organisationId);
    Task<UserOrganisationRoleDTO> Assign(AssignUserRoleDTO dto);
    Task Remove(Guid organisationId, Guid id);
    Task UpdateRole(Guid organisationId, Guid id, string roleName);
}
