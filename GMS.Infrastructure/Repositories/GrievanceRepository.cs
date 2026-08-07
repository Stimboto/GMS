using GMS.Application.Interfaces;
using GMS.Domain.Entities;
using GMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GMS.Infrastructure.Repositories;

public class GrievanceRepository : GenericRepository<Grievance>, IGrievanceRepository
{
    public GrievanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Grievance?> GetGrievanceWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Department)
            .Include(g => g.SubmittedByUser)
            .Include(g => g.AssignedOfficer)
            .Include(g => g.StatusHistories)
                .ThenInclude(sh => sh.ChangedByUser)
            .Include(g => g.StatusHistories)
                .ThenInclude(sh => sh.Attachment)
            .Include(g => g.Attachments)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Grievance>> GetGrievancesByCitizenAsync(int citizenId)
    {
        return await _dbSet
            .Include(g => g.Department)
            .Include(g => g.SubmittedByUser)
            .Include(g => g.AssignedOfficer)
            .Where(g => g.SubmittedByUserId == citizenId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetGrievancesByOfficerAsync(int officerId)
    {
        return await _dbSet
            .Include(g => g.Department)
            .Include(g => g.SubmittedByUser)
            .Include(g => g.AssignedOfficer)
            .Where(g => g.AssignedOfficerId == officerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Grievance>> GetAllGrievancesWithDetailsAsync()
    {
        return await _dbSet
            .Include(g => g.Department)
            .Include(g => g.SubmittedByUser)
            .Include(g => g.AssignedOfficer)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }
}
