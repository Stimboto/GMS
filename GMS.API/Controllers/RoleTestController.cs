using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/test")]
public class RoleTestController : BaseApiController
{
    [HttpGet("citizen")]
    [Authorize(Policy = "CitizenPolicy")]
    public IActionResult CitizenEndpoint()
    {
        return Ok(new { message = "Citizen access granted." });
    }

    [HttpGet("officer")]
    [Authorize(Policy = "OfficerPolicy")]
    public IActionResult OfficerEndpoint()
    {
        return Ok(new { message = "Officer access granted." });
    }

    [HttpGet("admin")]
    [Authorize(Policy = "AdminPolicy")]
    public IActionResult AdminEndpoint()
    {
        return Ok(new { message = "Admin access granted." });
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult ProfileEndpoint()
    {
        return Ok(new
        {
            UserId = CurrentUserId,
            FullName = CurrentUserService.FullName,
            Email = CurrentUserEmail,
            Role = CurrentUserRole
        });
    }
}
