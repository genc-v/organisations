using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IUserOrganisationRoleService
{
    Task<List<UserOrganisationRoleDTO>> GetByOrganisation(Guid organisationId);
    Task<UserOrganisationRoleDTO> Assign(AssignUserRoleDTO dto);
    Task Remove(Guid id);
}
