using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IOrganisationApiKeyService
{
    Task<List<OrganisationApiKeyDTO>> GetByOrganisation(Guid organisationId);
    Task<OrganisationApiKeyDTO> Create(Guid organisationId, CreateOrganisationApiKeyDTO dto);
    Task Toggle(Guid id);
    Task Delete(Guid id);
}
