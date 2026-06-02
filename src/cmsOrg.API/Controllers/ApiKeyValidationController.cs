using cmsOrg.Application.Common;
using cmsOrg.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cmsOrg.API.Controllers;

[ApiController]
[Route("api-keys")]
public class ApiKeyValidationController(IOrganisationApiKeyService service) : ControllerBase
{
    [HttpGet("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate([FromHeader(Name = "X-Api-Key")] string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw AppException.Unauthorized("Missing API key.");

        return Ok(await service.Validate(apiKey));
    }
}
