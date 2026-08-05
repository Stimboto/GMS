using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GMS.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count = 10)
    {
        return await _dbSet
            .OrderByDescending(a => a.PerformedOn)
            .Take(count)
            .ToListAsync();
    }
}
