using cmsOrg.Application.DTO;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("organisations/{organisationId:guid}/members")]
[Authorize]
public class UserOrganisationRoleController(IUserOrganisationRoleService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organisationId)
        => Ok(await service.GetByOrganisation(organisationId));

    [HttpPost]
    public async Task<IActionResult> Assign(Guid organisationId, [FromBody] AssignUserRoleDTO dto)
        => Ok(await service.Assign(dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid organisationId, Guid id)
    {
        await service.Remove(id);
        return NoContent();
    }
}
