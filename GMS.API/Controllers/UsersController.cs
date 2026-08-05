using GMS.Application.DTOs.Users;
using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMS.API.Controllers;

[Authorize(Policy = "AdminPolicy")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        await _userService.UpdateUserRoleAsync(id, request, performedBy);
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
    {
        var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        await _userService.UpdateUserStatusAsync(id, request, performedBy);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
        await _userService.DeleteUserAsync(id, performedBy);
        return NoContent();
    }
}
