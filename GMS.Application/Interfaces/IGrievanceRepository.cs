using GMS.Domain.Entities;

namespace GMS.Application.Interfaces;

public interface IGrievanceRepository : IGenericRepository<Grievance>
{
    Task<Grievance?> GetGrievanceWithDetailsAsync(int id);
    Task<IEnumerable<Grievance>> GetGrievancesByCitizenAsync(int citizenId);
    Task<IEnumerable<Grievance>> GetGrievancesByOfficerAsync(int officerId);
    Task<IEnumerable<Grievance>> GetAllGrievancesWithDetailsAsync();
}
