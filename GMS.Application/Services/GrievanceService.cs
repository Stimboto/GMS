using GMS.Application.DTOs.Grievances;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Domain.Enums;

namespace GMS.Application.Services;

public class GrievanceService : IGrievanceService
{
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IGenericRepository<Department> _departmentRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IOllamaService _ollamaService;
    private readonly IRealTimeNotifier _notifier;
    private readonly IAuditLogService _auditLogService;

    public GrievanceService(
        IGrievanceRepository grievanceRepository,
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<User> userRepository,
        INotificationService notificationService,
        IOllamaService ollamaService,
        IRealTimeNotifier notifier,
        IAuditLogService auditLogService)
    {
        _grievanceRepository = grievanceRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _ollamaService = ollamaService;
        _notifier = notifier;
        _auditLogService = auditLogService;
    }

    private GrievanceResponse MapToResponse(Grievance grievance)
    {
        return new GrievanceResponse
        {
            Id = grievance.Id,
            TrackingId = grievance.TrackingId,
            Title = grievance.Title,
            Description = grievance.Description,
            Status = grievance.Status,
            Priority = grievance.Priority,
            Category = grievance.Category,
            Department = grievance.Department?.DepartmentName ?? "Unknown",
            SubmittedBy = grievance.SubmittedByUser?.FullName ?? "Unknown",
            AssignedOfficer = grievance.AssignedOfficer?.FullName,
            CreatedAt = grievance.CreatedAt,
            UpdatedAt = grievance.UpdatedAt,
            SatisfactionRating = grievance.SatisfactionRating,
            FeedbackRemarks = grievance.FeedbackRemarks
        };
    }

    public async Task<GrievanceResponse> CreateAsync(CreateGrievanceRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Description is required.");
        if (string.IsNullOrWhiteSpace(request.Category)) throw new ArgumentException("Category is required.");

        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
        if (department == null) throw new ArgumentException("Department not found.");

        var grievance = new Grievance
        {
            TrackingId = $"GMS-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
            Title = request.Title,
            Description = request.Description,
            Summary = "Pending AI Analysis...",
            DepartmentId = request.DepartmentId,
            Category = "Pending",
            Priority = GrievancePriority.Medium,
            SubmittedByUserId = userId,
            Status = GrievanceStatus.Submitted
        };

        var statusHistory = new StatusHistory
        {
            OldStatus = GrievanceStatus.Submitted,
            NewStatus = GrievanceStatus.Submitted,
            Remarks = "Grievance submitted by citizen.",
            ChangedByUserId = userId
        };
        grievance.StatusHistories.Add(statusHistory);

        var created = await _grievanceRepository.AddAsync(grievance);
        
        // Trigger AI analysis in background
        _ = _ollamaService.AnalyzeGrievanceAsync(created.Id, request.Description);

        var createdWithDetails = await _grievanceRepository.GetGrievanceWithDetailsAsync(created.Id);

        await _notificationService.CreateNotificationAsync(userId, "Grievance Submitted", $"Your grievance '{created.Title}' has been submitted successfully.");
        await _notifier.NotifyRoleAsync("Admin", "New Grievance", $"A new grievance '{created.Title}' requires assignment.");

        return MapToResponse(createdWithDetails!);
    }

    public async Task<IEnumerable<GrievanceResponse>> GetMyGrievancesAsync(int userId)
    {
        var grievances = await _grievanceRepository.GetGrievancesByCitizenAsync(userId);
        return grievances.Select(MapToResponse);
    }

    public async Task<GrievanceResponse> GetByIdAsync(int id, int userId, string userRole)
    {
        var grievance = await _grievanceRepository.GetGrievanceWithDetailsAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        if (userRole == "Citizen" && grievance.SubmittedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only view your own grievances.");
        }

        if (userRole == "Officer" && grievance.AssignedOfficerId != userId)
        {
            throw new UnauthorizedAccessException("You can only view grievances assigned to you.");
        }

        return MapToResponse(grievance);
    }

    public async Task UpdateAsync(int id, UpdateGrievanceRequest request, int userId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        if (grievance.SubmittedByUserId != userId)
            throw new UnauthorizedAccessException("You can only update your own grievances.");

        if (grievance.Status != GrievanceStatus.Submitted)
            throw new InvalidOperationException("You can only update a grievance when its status is Submitted.");

        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
        if (department == null) throw new ArgumentException("Department not found.");

        grievance.Title = request.Title;
        grievance.Description = request.Description;
        grievance.DepartmentId = request.DepartmentId;
        grievance.Category = request.Category;

        await _grievanceRepository.UpdateAsync(grievance);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        if (grievance.SubmittedByUserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own grievances.");

        if (grievance.Status != GrievanceStatus.Submitted)
            throw new InvalidOperationException("You can only delete a grievance when its status is Submitted.");

        await _grievanceRepository.DeleteAsync(grievance);
    }

    public async Task<IEnumerable<GrievanceResponse>> GetAssignedGrievancesAsync(int officerId)
    {
        var grievances = await _grievanceRepository.GetGrievancesByOfficerAsync(officerId);
        return grievances.Select(MapToResponse);
    }

    public async Task UpdateStatusAsync(int id, UpdateStatusRequest request, int officerId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        if (grievance.AssignedOfficerId != officerId)
            throw new UnauthorizedAccessException("You can only update status for grievances assigned to you.");

        // Validate state transition
        if (!IsValidTransition(grievance.Status, request.Status))
        {
            throw new InvalidOperationException($"Cannot transition status from {grievance.Status} to {request.Status}");
        }

        var history = new StatusHistory
        {
            GrievanceId = grievance.Id,
            OldStatus = grievance.Status,
            NewStatus = request.Status,
            Remarks = request.Remarks,
            ImageUrl = request.ImageUrl,
            ChangedByUserId = officerId
        };
        
        grievance.Status = request.Status;
        
        grievance.StatusHistories.Add(history);
        await _grievanceRepository.UpdateAsync(grievance);

        string message = $"The status of your grievance '{grievance.TrackingId}' has been updated to {request.Status}.";
        await _notificationService.CreateNotificationAsync(grievance.SubmittedByUserId, "Grievance Status Updated", message);
        await _notifier.NotifyUserAsync(grievance.SubmittedByUserId, "Status Update", message);
    }

    private bool IsValidTransition(GrievanceStatus current, GrievanceStatus next)
    {
        return true; // Simplify transitions for enterprise logic overrides (like Reopen)
    }

    public async Task SubmitFeedbackAsync(int id, SubmitFeedbackRequest request, int userId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        if (grievance.SubmittedByUserId != userId)
            throw new UnauthorizedAccessException("You can only submit feedback for your own grievances.");

        if (grievance.Status != GrievanceStatus.Resolved && grievance.Status != GrievanceStatus.Closed)
            throw new InvalidOperationException("Grievance must be resolved before providing feedback.");

        grievance.SatisfactionRating = request.Rating;
        grievance.FeedbackRemarks = request.Remarks;

        if (request.Rating <= 2)
        {
            // Business Rule: Reopen the grievance
            var history = new StatusHistory
            {
                GrievanceId = grievance.Id,
                OldStatus = grievance.Status,
                NewStatus = GrievanceStatus.Reopened,
                Remarks = $"Citizen was dissatisfied (Rating: {request.Rating}/5). Reopening grievance.",
                ChangedByUserId = userId
            };
            grievance.Status = GrievanceStatus.Reopened;
            grievance.StatusHistories.Add(history);

            if (grievance.AssignedOfficerId.HasValue)
            {
                string msg = $"Grievance '{grievance.TrackingId}' was reopened due to low rating.";
                await _notificationService.CreateNotificationAsync(grievance.AssignedOfficerId.Value, "Grievance Reopened", msg);
                await _notifier.NotifyUserAsync(grievance.AssignedOfficerId.Value, "Grievance Reopened", msg);
            }
            await _notifier.NotifyRoleAsync("Admin", "Grievance Reopened", $"Grievance '{grievance.TrackingId}' was reopened.");
        }
        else
        {
            if (grievance.Status != GrievanceStatus.Closed)
            {
                var history = new StatusHistory
                {
                    GrievanceId = grievance.Id,
                    OldStatus = grievance.Status,
                    NewStatus = GrievanceStatus.Closed,
                    Remarks = $"Citizen is satisfied (Rating: {request.Rating}/5). Closing grievance.",
                    ChangedByUserId = userId
                };
                grievance.Status = GrievanceStatus.Closed;
                grievance.StatusHistories.Add(history);
            }
        }

        await _grievanceRepository.UpdateAsync(grievance);
    }

    public async Task<IEnumerable<GrievanceResponse>> GetAllAsync()
    {
        var grievances = await _grievanceRepository.GetAllGrievancesWithDetailsAsync();
        return grievances.Select(MapToResponse);
    }

    public async Task AssignOfficerAsync(int id, AssignOfficerRequest request)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        var officer = await _userRepository.GetByIdAsync(request.OfficerId);
        if (officer == null) throw new ArgumentException("Officer not found.");

        // An Admin can assign directly from Submitted or re-assign from Assigned/InReview
        // But for standard flow, let's allow it as long as it's not Closed
        if (grievance.Status == GrievanceStatus.Closed || grievance.Status == GrievanceStatus.Resolved)
        {
            throw new InvalidOperationException("Cannot assign an officer to a resolved or closed grievance.");
        }

        grievance.AssignedOfficerId = request.OfficerId;
        if (grievance.Status == GrievanceStatus.Submitted)
        {
            grievance.Status = GrievanceStatus.Assigned;
        }

        await _grievanceRepository.UpdateAsync(grievance);

        string msg = $"You have been assigned a new grievance: '{grievance.TrackingId}'.";
        await _notificationService.CreateNotificationAsync(request.OfficerId, "New Grievance Assigned", msg);
        await _notifier.NotifyUserAsync(request.OfficerId, "New Assignment", msg);
    }

    public async Task UpdateDepartmentAsync(int id, int departmentId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) throw new ArgumentException("Department not found.");

        grievance.DepartmentId = departmentId;
        await _grievanceRepository.UpdateAsync(grievance);

        if (grievance.AssignedOfficerId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(grievance.AssignedOfficerId.Value, "Department Changed", $"The department for grievance '{grievance.Title}' has been changed to {department.DepartmentName}.");
        }
    }
}
