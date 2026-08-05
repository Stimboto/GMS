using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/profile")]
[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;

    public ProfileController(IUserService userService, IFileStorageService fileStorageService)
    {
        _userService = userService;
        _fileStorageService = fileStorageService;
    }

    [HttpPut("image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Error = "No file uploaded." });

        try
        {
            var user = await _userService.GetUserByIdAsync(CurrentUserId);
            
            // Delete old profile image if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                _fileStorageService.DeleteFile(user.ProfileImageUrl);
            }

            using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            await _userService.UpdateProfileImageAsync(CurrentUserId, url);

            return Ok(new { ProfileImageUrl = url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        try
        {
            await _userService.UpdatePreferencesAsync(CurrentUserId, request.EmailNotificationsEnabled);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

public class UpdatePreferencesRequest
{
    public bool EmailNotificationsEnabled { get; set; }
}
