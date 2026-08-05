using GMS.Domain.Entities;

namespace GMS.Application.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count = 10);
}
