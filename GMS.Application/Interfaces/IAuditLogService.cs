using GMS.Domain.Entities;

namespace GMS.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string performedBy, string? oldValue = null, string? newValue = null);
    Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count = 10);
}
