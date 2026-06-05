using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations/{organisationId:guid}/members")]
[Authorize]
public class UserOrganisationRoleController(IUserOrganisationRoleService service, IOrganisationService organisationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organisationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        await organisationService.CheckAccess(organisationId, "Viewer");
        return Ok(await service.GetByOrganisation(organisationId, page, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> Assign(Guid organisationId, [FromBody] AssignUserRoleDTO dto)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        dto.OrganisationId = organisationId;
        return Ok(await service.Assign(dto));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid organisationId, Guid id)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        await service.Remove(organisationId, id);
        return NoContent();
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid organisationId, Guid id, [FromBody] string roleName)
    {
        await organisationService.CheckAccess(organisationId, "Admin");
        await service.UpdateRole(organisationId, id, roleName);
        return NoContent();
    }
}
