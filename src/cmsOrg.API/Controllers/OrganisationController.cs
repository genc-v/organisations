using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations")]
[Authorize]
public class OrganisationController(IOrganisationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => Ok(await service.GetAll(page, pageSize, search));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await service.GetById(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganisationDTO dto)
        => Ok(await service.Create(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganisationDTO dto)
    {
        await service.Update(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.Delete(id);
        return NoContent();
    }
}
