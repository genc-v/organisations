using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IOrganisationService
{
    Task<PaginatedResult<OrganisationDTO>> GetAll(int page, int pageSize, string? search);
    Task<OrganisationDTO> GetById(Guid id);
    Task<OrganisationDTO> Create(CreateOrganisationDTO dto);
    Task Update(Guid id, UpdateOrganisationDTO dto);
    Task Delete(Guid id);
}
