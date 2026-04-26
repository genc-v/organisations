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
    {
        await service.CheckAccess(id, "Viewer");
        return Ok(await service.GetById(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganisationDTO dto)
        => Ok(await service.Create(dto));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganisationDTO dto)
    {
        await service.CheckAccess(id, "Admin");
        await service.Update(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.CheckAccess(id, "Admin");
        await service.Delete(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/role")]
    public async Task<IActionResult> GetMyRole(Guid id)
    {
        var role = await service.GetMyRole(id);
        return Ok(new { Role = role });
    }

    [HttpGet("permissions")]
    [AllowAnonymous]
    public IActionResult GetPermissions()
    {
        return Ok(cmsOrg.Domain.Entities.Permission.DefaultPermissions);
    }
}
