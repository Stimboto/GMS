using GMS.Application.DTOs.Grievances;
using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/grievances")]
public class GrievancesController : BaseApiController
{
    private readonly IGrievanceService _grievanceService;

    public GrievancesController(IGrievanceService grievanceService)
    {
        _grievanceService = grievanceService;
    }

    // ==========================================
    // CITIZEN FEATURES
    // ==========================================

    [HttpPost]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> Create([FromBody] CreateGrievanceRequest request)
    {
        try
        {
            var response = await _grievanceService.CreateAsync(request, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> GetMyGrievances()
    {
        var grievances = await _grievanceService.GetMyGrievancesAsync(CurrentUserId);
        return Ok(grievances);
    }

    [HttpGet("{id}")]
    [Authorize] // Any authenticated user can access, but service layer validates permissions
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var grievance = await _grievanceService.GetByIdAsync(id, CurrentUserId, CurrentUserRole);
            return Ok(grievance);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGrievanceRequest request)
    {
        try
        {
            await _grievanceService.UpdateAsync(id, request, CurrentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _grievanceService.DeleteAsync(id, CurrentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/feedback")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> SubmitFeedback(int id, [FromBody] SubmitFeedbackRequest request)
    {
        try
        {
            await _grievanceService.SubmitFeedbackAsync(id, request, CurrentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // OFFICER FEATURES
    // ==========================================

    [HttpGet("assigned")]
    [Authorize(Policy = "OfficerPolicy")]
    public async Task<IActionResult> GetAssignedGrievances()
    {
        var grievances = await _grievanceService.GetAssignedGrievancesAsync(CurrentUserId);
        return Ok(grievances);
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "OfficerPolicy")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            await _grievanceService.UpdateStatusAsync(id, request, CurrentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==========================================
    // ADMIN FEATURES
    // ==========================================

    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetAll()
    {
        var grievances = await _grievanceService.GetAllAsync();
        return Ok(grievances);
    }

    [HttpPut("{id}/assign")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> AssignOfficer(int id, [FromBody] AssignOfficerRequest request)
    {
        try
        {
            await _grievanceService.AssignOfficerAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/department")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] int departmentId)
    {
        try
        {
            await _grievanceService.UpdateDepartmentAsync(id, departmentId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
