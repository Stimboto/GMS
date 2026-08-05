using GMS.Application.DTOs.Grievances;

namespace GMS.Application.Interfaces;

public interface IGrievanceService
{
    // Citizen
    Task<GrievanceResponse> CreateAsync(CreateGrievanceRequest request, int userId);
    Task<IEnumerable<GrievanceResponse>> GetMyGrievancesAsync(int userId);
    Task<GrievanceResponse> GetByIdAsync(int id, int userId, string userRole);
    Task UpdateAsync(int id, UpdateGrievanceRequest request, int userId);
    Task DeleteAsync(int id, int userId);
    Task SubmitFeedbackAsync(int id, SubmitFeedbackRequest request, int userId);

    // Officer
    Task<IEnumerable<GrievanceResponse>> GetAssignedGrievancesAsync(int officerId);
    Task UpdateStatusAsync(int id, UpdateStatusRequest request, int officerId);

    // Admin
    Task<IEnumerable<GrievanceResponse>> GetAllAsync();
    Task AssignOfficerAsync(int id, AssignOfficerRequest request);
    Task UpdateDepartmentAsync(int id, int departmentId);
}
