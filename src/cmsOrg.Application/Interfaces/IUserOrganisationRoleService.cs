using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IUserOrganisationRoleService
{
    Task<PaginatedResult<UserOrganisationRoleDTO>> GetByOrganisation(Guid organisationId, int page, int pageSize);
    Task<UserOrganisationRoleDTO> Assign(AssignUserRoleDTO dto);
    Task Remove(Guid organisationId, Guid id);
    Task UpdateRole(Guid organisationId, Guid id, string roleName);
}
