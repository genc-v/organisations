using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations/{organisationId:guid}/api-keys")]
[Authorize]
public class OrganisationApiKeyController(IOrganisationApiKeyService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organisationId)
        => Ok(await service.GetByOrganisation(organisationId));

    [HttpPost]
    public async Task<IActionResult> Create(Guid organisationId, [FromBody] CreateOrganisationApiKeyDTO dto)
        => Ok(await service.Create(organisationId, dto));

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid organisationId, Guid id)
    {
        await service.Toggle(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid organisationId, Guid id)
    {
        await service.Delete(id);
        return NoContent();
    }
}
