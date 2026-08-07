using GMS.Application.DTOs.Department;

namespace GMS.Application.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto request, CancellationToken cancellationToken = default);
    Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto request, CancellationToken cancellationToken = default);
    Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
    Task AssignOfficerAsync(int departmentId, int officerId, CancellationToken cancellationToken = default);
}
