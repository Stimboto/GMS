using GMS.Domain.Common;

namespace GMS.Domain.Entities;

public class Department : BaseEntity
{
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
}
