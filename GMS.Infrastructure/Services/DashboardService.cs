using GMS.Application.DTOs.Dashboard;
using GMS.Application.DTOs.Grievances;
using GMS.Application.DTOs.Notifications;
using GMS.Application.Interfaces;
using GMS.Domain.Enums;
using GMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CitizenDashboardDto> GetCitizenDashboardAsync(int citizenId)
    {
        var grievances = await _context.Grievances
            .AsNoTracking()
            .Where(g => g.SubmittedByUserId == citizenId)
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.Status,
                g.Priority,
                g.Category,
                DepartmentName = g.Department.DepartmentName,
                SubmittedByName = g.SubmittedByUser.FullName,
                AssignedOfficerName = g.AssignedOfficer != null ? g.AssignedOfficer.FullName : null,
                g.CreatedAt,
                g.UpdatedAt
            })
            .ToListAsync();

        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == citizenId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        var latestGrievances = grievances
            .OrderByDescending(g => g.CreatedAt)
            .Take(10)
            .Select(g => new GrievanceResponse
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                Priority = g.Priority,
                Category = g.Category,
                Department = g.DepartmentName,
                SubmittedBy = g.SubmittedByName,
                AssignedOfficer = g.AssignedOfficerName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            });

        return new CitizenDashboardDto
        {
            TotalGrievances = grievances.Count,
            Submitted = grievances.Count(g => g.Status == GrievanceStatus.Submitted),
            Assigned = grievances.Count(g => g.Status == GrievanceStatus.Assigned),
            InReview = grievances.Count(g => g.Status == GrievanceStatus.InReview),
            Resolved = grievances.Count(g => g.Status == GrievanceStatus.Resolved),
            Closed = grievances.Count(g => g.Status == GrievanceStatus.Closed),
            LatestGrievances = latestGrievances,
            LatestNotifications = notifications
        };
    }

    public async Task<OfficerDashboardDto> GetOfficerDashboardAsync(int officerId)
    {
        var grievances = await _context.Grievances
            .AsNoTracking()
            .Where(g => g.AssignedOfficerId == officerId)
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.Status,
                g.Priority,
                g.Category,
                DepartmentName = g.Department.DepartmentName,
                SubmittedByName = g.SubmittedByUser.FullName,
                AssignedOfficerName = g.AssignedOfficer!.FullName,
                g.CreatedAt,
                g.UpdatedAt,
                Histories = g.StatusHistories.Select(h => new { h.NewStatus, h.ChangedAt })
            })
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        var resolvedToday = grievances.Count(g => g.Histories.Any(h => h.NewStatus == GrievanceStatus.Resolved && h.ChangedAt.Date == today));
        var closedToday = grievances.Count(g => g.Histories.Any(h => h.NewStatus == GrievanceStatus.Closed && h.ChangedAt.Date == today));

        var latestAssigned = grievances
            .Where(g => g.Status == GrievanceStatus.Assigned || g.Status == GrievanceStatus.InReview)
            .OrderByDescending(g => g.CreatedAt)
            .Take(10)
            .Select(g => new GrievanceResponse
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Status = g.Status,
                Priority = g.Priority,
                Category = g.Category,
                Department = g.DepartmentName,
                SubmittedBy = g.SubmittedByName,
                AssignedOfficer = g.AssignedOfficerName,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            });

        return new OfficerDashboardDto
        {
            AssignedGrievances = grievances.Count,
            InReview = grievances.Count(g => g.Status == GrievanceStatus.InReview),
            ResolvedToday = resolvedToday,
            ClosedToday = closedToday,
            Pending = grievances.Count(g => g.Status == GrievanceStatus.Submitted || g.Status == GrievanceStatus.Assigned || g.Status == GrievanceStatus.InReview),
            LatestAssigned = latestAssigned
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var roleCounts = await _context.Users
            .AsNoTracking()
            .GroupBy(u => u.Role.Name)
            .Select(g => new { RoleName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleName, x => x.Count);

        var grievanceStats = await _context.Grievances
            .AsNoTracking()
            .GroupBy(g => g.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var totalGrievances = grievanceStats.Values.Sum();

        var deptStats = await _context.Departments
            .AsNoTracking()
            .Select(d => new DepartmentStatisticsDto
            {
                DepartmentName = d.DepartmentName,
                TotalGrievances = d.Grievances.Count,
                Resolved = d.Grievances.Count(g => g.Status == GrievanceStatus.Resolved || g.Status == GrievanceStatus.Closed),
                Pending = d.Grievances.Count(g => g.Status == GrievanceStatus.Submitted || g.Status == GrievanceStatus.Assigned || g.Status == GrievanceStatus.InReview)
            })
            .ToListAsync();

        var officerStats = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role.Name == "Officer")
            .Select(o => new OfficerStatisticsDto
            {
                OfficerName = o.FullName,
                Assigned = _context.Grievances.Count(g => g.AssignedOfficerId == o.Id),
                Resolved = _context.Grievances.Count(g => g.AssignedOfficerId == o.Id && (g.Status == GrievanceStatus.Resolved || g.Status == GrievanceStatus.Closed)),
                Pending = _context.Grievances.Count(g => g.AssignedOfficerId == o.Id && (g.Status == GrievanceStatus.Submitted || g.Status == GrievanceStatus.Assigned || g.Status == GrievanceStatus.InReview))
            })
            .ToListAsync();

        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var inactiveUsers = await _context.Users.CountAsync(u => !u.IsActive);

        var today = DateTime.UtcNow.Date;
        var todaysNew = await _context.Grievances.CountAsync(g => g.CreatedAt.Date == today);
        var todaysResolved = await _context.StatusHistories.CountAsync(h => h.NewStatus == GrievanceStatus.Resolved && h.ChangedAt.Date == today);
        var todaysClosed = await _context.StatusHistories.CountAsync(h => h.NewStatus == GrievanceStatus.Closed && h.ChangedAt.Date == today);

        return new AdminDashboardDto
        {
            TotalUsers = roleCounts.Values.Sum(),
            Citizens = roleCounts.GetValueOrDefault("Citizen", 0),
            Officers = roleCounts.GetValueOrDefault("Officer", 0),
            Admins = roleCounts.GetValueOrDefault("Admin", 0),
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            TotalGrievances = totalGrievances,
            Submitted = grievanceStats.GetValueOrDefault(GrievanceStatus.Submitted, 0),
            Assigned = grievanceStats.GetValueOrDefault(GrievanceStatus.Assigned, 0),
            InReview = grievanceStats.GetValueOrDefault(GrievanceStatus.InReview, 0),
            Resolved = grievanceStats.GetValueOrDefault(GrievanceStatus.Resolved, 0),
            Closed = grievanceStats.GetValueOrDefault(GrievanceStatus.Closed, 0),
            TodaysNew = todaysNew,
            TodaysResolved = todaysResolved,
            TodaysClosed = todaysClosed,
            DepartmentStatistics = deptStats,
            OfficerStatistics = officerStats
        };
    }

    public async Task<ChartDataDto> GetStatusChartDataAsync()
    {
        var data = await _context.Grievances
            .AsNoTracking()
            .GroupBy(g => g.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return new ChartDataDto
        {
            Labels = data.Select(d => d.Status),
            Values = data.Select(d => d.Count)
        };
    }

    public async Task<ChartDataDto> GetDepartmentChartDataAsync()
    {
        var data = await _context.Departments
            .AsNoTracking()
            .Select(d => new { d.DepartmentName, Count = d.Grievances.Count })
            .Where(d => d.Count > 0)
            .ToListAsync();

        return new ChartDataDto
        {
            Labels = data.Select(d => d.DepartmentName),
            Values = data.Select(d => d.Count)
        };
    }

    public async Task<IEnumerable<MonthlyGrievanceDto>> GetMonthlyGrievancesAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        
        var rawData = await _context.Grievances
            .AsNoTracking()
            .Where(g => g.CreatedAt.Year == currentYear)
            .Select(g => new { g.CreatedAt.Month })
            .ToListAsync();
            
        var grouped = rawData
            .GroupBy(g => g.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToDictionary(g => g.Month, g => g.Count);

        var result = new List<MonthlyGrievanceDto>();
        for (int i = 1; i <= 12; i++)
        {
            result.Add(new MonthlyGrievanceDto
            {
                Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                Count = grouped.GetValueOrDefault(i, 0)
            });
        }

        return result;
    }
}
