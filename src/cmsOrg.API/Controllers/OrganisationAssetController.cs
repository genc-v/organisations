using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations/{organisationId:guid}/assets")]
[Authorize]
public class OrganisationAssetController(IOrganisationAssetService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organisationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Ok(await service.GetByOrganisation(organisationId, page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid organisationId, Guid id)
        => Ok(await service.GetById(id));

    [HttpPost]
    public async Task<IActionResult> Create(Guid organisationId, [FromBody] CreateOrganisationAssetDTO dto)
        => Ok(await service.Create(organisationId, dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid organisationId, Guid id, [FromBody] UpdateOrganisationAssetDTO dto)
    {
        await service.Update(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid organisationId, Guid id)
    {
        await service.Delete(id);
        return NoContent();
    }
}
