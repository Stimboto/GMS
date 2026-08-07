using GMS.Application.DTOs.Department;
using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/departments")]
public class DepartmentController : BaseApiController
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllDepartments(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
            return Ok(department);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto request, CancellationToken cancellationToken)
    {
        var result = await _departmentService.CreateDepartmentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _departmentService.UpdateDepartmentAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/assign-officer")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> AssignOfficer(int id, [FromBody] AssignOfficerDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _departmentService.AssignOfficerAsync(id, request.OfficerId, cancellationToken);
            return Ok(new { Message = "Officer assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
    }
}
