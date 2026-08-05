using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private ICurrentUserService? _currentUserService;

    protected ICurrentUserService CurrentUserService => 
        _currentUserService ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    protected int CurrentUserId => CurrentUserService.UserId;
    protected string CurrentUserEmail => CurrentUserService.Email;
    protected string CurrentUserRole => CurrentUserService.Role;
}
