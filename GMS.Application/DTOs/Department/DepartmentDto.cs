namespace GMS.Application.DTOs.Department;

public class DepartmentDto
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateDepartmentDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateDepartmentDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignOfficerDto
{
    public int OfficerId { get; set; }
}
