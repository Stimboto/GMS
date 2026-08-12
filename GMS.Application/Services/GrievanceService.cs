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
    private readonly IFileStorageService _fileStorageService;

    public GrievanceService(
        IGrievanceRepository grievanceRepository,
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<User> userRepository,
        INotificationService notificationService,
        IOllamaService ollamaService,
        IRealTimeNotifier notifier,
        IAuditLogService auditLogService,
        IFileStorageService fileStorageService)
    {
        _grievanceRepository = grievanceRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _ollamaService = ollamaService;
        _notifier = notifier;
        _auditLogService = auditLogService;
        _fileStorageService = fileStorageService;
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
            FeedbackRemarks = grievance.FeedbackRemarks,
            Summary = grievance.Summary ?? string.Empty,
            Attachments = grievance.Attachments?.Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                UploadedAt = a.CreatedAt,
                DownloadUrl = a.FilePath
            }).ToList() ?? new List<AttachmentResponse>(),
            StatusHistories = grievance.StatusHistories?.Select(sh => new StatusHistoryResponse
            {
                Id = sh.Id,
                OldStatus = sh.OldStatus,
                NewStatus = sh.NewStatus,
                Remarks = sh.Remarks,
                IsInternal = sh.IsInternal,
                AttachmentUrl = sh.Attachment?.FilePath,
                AttachmentName = sh.Attachment?.FileName,
                ChangedAt = sh.ChangedAt,
                ChangedByUserId = sh.ChangedByUserId,
                ChangedByUserName = sh.ChangedByUser?.FullName ?? "Unknown"
            }).ToList() ?? new List<StatusHistoryResponse>()
        };
    }

    public async Task<GrievanceResponse> CreateAsync(CreateGrievanceRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Description is required.");

        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);
        if (department == null) throw new ArgumentException("Department not found.");

        var categoryName = !string.IsNullOrWhiteSpace(request.Category) && request.Category != "Pending"
            ? request.Category
            : department.DepartmentName;

        var hasAiPriority = !string.IsNullOrWhiteSpace(request.Priority) && request.Priority != "Pending";

        var grievance = new Grievance
        {
            TrackingId = $"GMS-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
            Title = request.Title,
            Description = request.Description,
            Summary = !string.IsNullOrWhiteSpace(request.Summary) ? request.Summary : request.Description,
            DepartmentId = request.DepartmentId,
            Category = categoryName,
            Priority = hasAiPriority && Enum.TryParse<GrievancePriority>(request.Priority, true, out var p) ? p : GrievancePriority.Medium,
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

        if (request.File != null)
        {
            using var stream = request.File.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.File.ContentType);
            var attachment = new Attachment
            {
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                // GrievanceId will be set after AddAsync or we just leave it to EF navigation properties
                // but since Grievance doesn't have an ID yet, we just add it to the collection
            };
            grievance.Attachments.Add(attachment);
            statusHistory.Attachment = attachment;
        }

        grievance.StatusHistories.Add(statusHistory);

        var created = await _grievanceRepository.AddAsync(grievance);
        
        // Trigger AI analysis in background ONLY if frontend didn't already provide priority/summary
        if (!hasAiPriority)
        {
            _ = _ollamaService.AnalyzeGrievanceAsync(created.Id, request.Description);
        }

        var createdWithDetails = await _grievanceRepository.GetGrievanceWithDetailsAsync(created.Id);

        await _notificationService.CreateNotificationAsync(userId, "Grievance Submitted", $"Your grievance '{created.Title}' has been submitted successfully.");
        
        var admins = await _userRepository.FindAsync(u => u.Role.Name == "Admin");
        foreach(var admin in admins)
        {
            await _notificationService.CreateNotificationAsync(admin.Id, "New Grievance", $"A new grievance '{created.Title}' requires assignment.");
        }
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

        if (userRole == "Citizen")
        {
            grievance.StatusHistories = grievance.StatusHistories.Where(sh => !sh.IsInternal).ToList();
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

        var user = await _userRepository.GetByIdAsync(officerId);
        bool isAdmin = user != null && user.Role?.Name == "Admin";

        if (!isAdmin && grievance.AssignedOfficerId != officerId)
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
            ChangedByUserId = officerId
        };
        
        if (request.File != null)
        {
            using var stream = request.File.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.File.ContentType);
            var attachment = new Attachment
            {
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                GrievanceId = grievance.Id
            };
            if (grievance.Attachments == null) grievance.Attachments = new List<Attachment>();
            grievance.Attachments.Add(attachment);
            history.Attachment = attachment;
        }

        grievance.Status = request.Status;
        
        grievance.StatusHistories.Add(history);
        await _grievanceRepository.UpdateAsync(grievance);

        string message = $"The status of your grievance '{grievance.TrackingId}' has been updated to {request.Status}.";
        await _notificationService.CreateNotificationAsync(grievance.SubmittedByUserId, "Grievance Status Updated", message);
        await _notifier.NotifyUserAsync(grievance.SubmittedByUserId, "Status Update", message);
    }

    public async Task AddRemarkAsync(int id, AddRemarkRequest request, int userId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null) throw new KeyNotFoundException("Grievance not found.");

        var history = new StatusHistory
        {
            GrievanceId = grievance.Id,
            OldStatus = grievance.Status,
            NewStatus = grievance.Status,
            Remarks = request.Remarks,
            IsInternal = request.IsInternal,
            ChangedByUserId = userId
        };

        if (request.File != null)
        {
            using var stream = request.File.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.File.ContentType);
            var attachment = new Attachment
            {
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                GrievanceId = grievance.Id
            };
            if (grievance.Attachments == null) grievance.Attachments = new List<Attachment>();
            grievance.Attachments.Add(attachment);
            history.Attachment = attachment;
        }

        grievance.StatusHistories.Add(history);
        await _grievanceRepository.UpdateAsync(grievance);
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

    public async Task AssignOfficerAsync(int id, AssignOfficerRequest request, int adminId)
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
        var oldStatus = grievance.Status;
        if (grievance.Status == GrievanceStatus.Submitted)
        {
            grievance.Status = GrievanceStatus.Assigned;
        }

        var history = new StatusHistory
        {
            OldStatus = oldStatus,
            NewStatus = grievance.Status,
            Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? $"Assigned to {officer.FullName}" : request.Remarks,
            IsInternal = request.IsInternal,
            ChangedByUserId = adminId
        };

        if (request.File != null)
        {
            using var stream = request.File.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, request.File.FileName, request.File.ContentType);
            var attachment = new Attachment
            {
                FileName = request.File.FileName,
                FilePath = filePath,
                ContentType = request.File.ContentType,
                GrievanceId = grievance.Id
            };
            if (grievance.Attachments == null) grievance.Attachments = new List<Attachment>();
            grievance.Attachments.Add(attachment);
            history.Attachment = attachment;
        }

        grievance.StatusHistories.Add(history);

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

    public async Task ToggleHistoryInternalAsync(int historyId, bool isInternal, int userId)
    {
        var success = await _grievanceRepository.ToggleHistoryInternalAsync(historyId, isInternal);
        if (!success) throw new KeyNotFoundException("History record not found.");
    }
}
