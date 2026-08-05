using GMS.Application.DTOs.Dashboard;

namespace GMS.Application.Interfaces;

public interface IDashboardService
{
    Task<CitizenDashboardDto> GetCitizenDashboardAsync(int citizenId);
    Task<OfficerDashboardDto> GetOfficerDashboardAsync(int officerId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    
    Task<ChartDataDto> GetStatusChartDataAsync();
    Task<ChartDataDto> GetDepartmentChartDataAsync();
    Task<IEnumerable<MonthlyGrievanceDto>> GetMonthlyGrievancesAsync();
}
