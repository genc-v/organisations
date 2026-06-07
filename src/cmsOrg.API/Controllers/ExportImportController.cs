using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Authorize]
public class ExportImportController(IExportImportService service) : ControllerBase
{
    [HttpGet("organisations/export")]
    public async Task<IActionResult> ExportOrganisations([FromQuery] string format = "json")
    {
        var (data, contentType, fileName) = await service.ExportOrganisations(format);
        return File(data, contentType, fileName);
    }

    [HttpGet("organisations/{organisationId:guid}/members/export")]
    public async Task<IActionResult> ExportMembers(Guid organisationId, [FromQuery] string format = "json")
    {
        var (data, contentType, fileName) = await service.ExportMembers(organisationId, format);
        return File(data, contentType, fileName);
    }
}
