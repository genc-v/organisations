using cmsOrg.Application.Common;
using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IOrganisationAssetService
{
    Task<PaginatedResult<OrganisationAssetDTO>> GetByOrganisation(Guid organisationId, int page, int pageSize);
    Task<OrganisationAssetDTO> GetById(Guid id);
    Task<OrganisationAssetDTO> Create(Guid organisationId, CreateOrganisationAssetDTO dto);
    Task Update(Guid id, UpdateOrganisationAssetDTO dto);
    Task Delete(Guid id);
}
