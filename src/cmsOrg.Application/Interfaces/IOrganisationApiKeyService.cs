using cmsOrg.Application.DTO;

namespace cmsOrg.Application.Interfaces;

public interface IOrganisationApiKeyService
{
    Task<ValidateApiKeyResultDTO> Validate(string key);
    Task<List<OrganisationApiKeyDTO>> GetByOrganisation(Guid organisationId);
    Task<OrganisationApiKeyDTO> Create(Guid organisationId, CreateOrganisationApiKeyDTO dto);
    Task Toggle(Guid organisationId, Guid id);
    Task Delete(Guid organisationId, Guid id);
}
