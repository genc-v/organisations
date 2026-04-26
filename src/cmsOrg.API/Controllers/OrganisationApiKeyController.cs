using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations/{organisationId:guid}/api-keys")]
[Authorize]
public class OrganisationApiKeyController(IOrganisationApiKeyService service, IOrganisationService organisationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organisationId)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        return Ok(await service.GetByOrganisation(organisationId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid organisationId, [FromBody] CreateOrganisationApiKeyDTO dto)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        return Ok(await service.Create(organisationId, dto));
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid organisationId, Guid id)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        await service.Toggle(organisationId, id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid organisationId, Guid id)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        await service.Delete(organisationId, id);
        return NoContent();
    }
}
