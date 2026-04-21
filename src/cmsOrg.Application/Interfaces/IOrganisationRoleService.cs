using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IOrganisationRoleService
{
    Task<PaginatedResult<OrganisationRoleDTO>> GetByOrganisation(Guid organisationId, int page, int pageSize);
    Task<OrganisationRoleDTO> GetById(Guid id);
    Task<OrganisationRoleDTO> Create(Guid organisationId, CreateOrganisationRoleDTO dto);
    Task Update(Guid id, UpdateOrganisationRoleDTO dto);
    Task Delete(Guid id);
}
