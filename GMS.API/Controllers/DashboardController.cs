using GMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMS.API.Controllers;

[Route("api/dashboard")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("citizen")]
    [Authorize(Policy = "CitizenPolicy")]
    public async Task<IActionResult> GetCitizenDashboard()
    {
        var data = await _dashboardService.GetCitizenDashboardAsync(CurrentUserId);
        return Ok(data);
    }

    [HttpGet("officer")]
    [Authorize(Policy = "OfficerPolicy")]
    public async Task<IActionResult> GetOfficerDashboard()
    {
        var data = await _dashboardService.GetOfficerDashboardAsync(CurrentUserId);
        return Ok(data);
    }

    [HttpGet("admin")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var data = await _dashboardService.GetAdminDashboardAsync();
        return Ok(data);
    }

    [HttpGet("charts/status")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetStatusChart()
    {
        var data = await _dashboardService.GetStatusChartDataAsync();
        return Ok(data);
    }

    [HttpGet("charts/departments")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetDepartmentChart()
    {
        var data = await _dashboardService.GetDepartmentChartDataAsync();
        return Ok(data);
    }

    [HttpGet("charts/monthly")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetMonthlyChart()
    {
        var data = await _dashboardService.GetMonthlyGrievancesAsync();
        return Ok(data);
    }
}
