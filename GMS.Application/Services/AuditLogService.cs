using GMS.Application.Interfaces;
using GMS.Domain.Entities;

namespace GMS.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(string action, string performedBy, string? oldValue = null, string? newValue = null)
    {
        var log = new AuditLog
        {
            Action = action,
            PerformedBy = performedBy,
            PerformedOn = DateTime.UtcNow,
            OldValue = oldValue,
            NewValue = newValue
        };

        await _auditLogRepository.AddAsync(log);
    }

    public async Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count = 10)
    {
        return await _auditLogRepository.GetLatestLogsAsync(count);
    }
}
