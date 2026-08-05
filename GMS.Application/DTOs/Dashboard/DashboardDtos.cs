using GMS.Application.DTOs.Grievances;
using GMS.Application.DTOs.Notifications;

namespace GMS.Application.DTOs.Dashboard;

public class CitizenDashboardDto
{
    public int TotalGrievances { get; set; }
    public int Submitted { get; set; }
    public int Assigned { get; set; }
    public int InReview { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    
    public IEnumerable<GrievanceResponse> LatestGrievances { get; set; } = new List<GrievanceResponse>();
    public IEnumerable<NotificationResponse> LatestNotifications { get; set; } = new List<NotificationResponse>();
}

public class OfficerDashboardDto
{
    public int AssignedGrievances { get; set; }
    public int InReview { get; set; }
    public int ResolvedToday { get; set; }
    public int ClosedToday { get; set; }
    public int Pending { get; set; } // Submitted, Assigned, InReview
    
    public IEnumerable<GrievanceResponse> LatestAssigned { get; set; } = new List<GrievanceResponse>();
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int Citizens { get; set; }
    public int Officers { get; set; }
    public int Admins { get; set; }
    
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }

    public int TotalGrievances { get; set; }
    public int Submitted { get; set; }
    public int Assigned { get; set; }
    public int InReview { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    
    public int TodaysNew { get; set; }
    public int TodaysResolved { get; set; }
    public int TodaysClosed { get; set; }

    public IEnumerable<DepartmentStatisticsDto> DepartmentStatistics { get; set; } = new List<DepartmentStatisticsDto>();
    public IEnumerable<OfficerStatisticsDto> OfficerStatistics { get; set; } = new List<OfficerStatisticsDto>();
}

public class DepartmentStatisticsDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int Resolved { get; set; }
    public int Pending { get; set; }
}

public class OfficerStatisticsDto
{
    public string OfficerName { get; set; } = string.Empty;
    public int Assigned { get; set; }
    public int Resolved { get; set; }
    public int Pending { get; set; }
}

public class ChartDataDto
{
    public IEnumerable<string> Labels { get; set; } = new List<string>();
    public IEnumerable<int> Values { get; set; } = new List<int>();
}

public class MonthlyGrievanceDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
