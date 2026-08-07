using GMS.Application.DTOs.Department;
using GMS.Application.Interfaces;
using GMS.Domain.Entities;

namespace GMS.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IGenericRepository<Department> _departmentRepository;
    private readonly IGenericRepository<User> _userRepository;

    public DepartmentService(
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<User> userRepository)
    {
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            DepartmentName = d.DepartmentName,
            Description = d.Description
        });
    }

    public async Task<DepartmentDto> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) throw new KeyNotFoundException("Department not found");

        return new DepartmentDto
        {
            Id = department.Id,
            DepartmentName = department.DepartmentName,
            Description = department.Description
        };
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto request, CancellationToken cancellationToken = default)
    {
        var department = new Department
        {
            DepartmentName = request.DepartmentName,
            Description = request.Description
        };

        var created = await _departmentRepository.AddAsync(department);

        return new DepartmentDto
        {
            Id = created.Id,
            DepartmentName = created.DepartmentName,
            Description = created.Description
        };
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(int id, UpdateDepartmentDto request, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) throw new KeyNotFoundException("Department not found");

        department.DepartmentName = request.DepartmentName;
        department.Description = request.Description;

        await _departmentRepository.UpdateAsync(department);

        return new DepartmentDto
        {
            Id = department.Id,
            DepartmentName = department.DepartmentName,
            Description = department.Description
        };
    }

    public async Task DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) throw new KeyNotFoundException("Department not found");

        await _departmentRepository.DeleteAsync(department);
    }

    public async Task AssignOfficerAsync(int departmentId, int officerId, CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null) throw new KeyNotFoundException("Department not found");

        var officer = await _userRepository.GetByIdAsync(officerId);
        if (officer == null) throw new KeyNotFoundException("Officer not found");
        
        // Ensure role check if necessary, assuming Officer RoleId is known, but generic repo doesn't load it by default unless configured
        // We'll assume the client sent a valid officer ID.

        officer.DepartmentId = departmentId;
        await _userRepository.UpdateAsync(officer);
    }
}
