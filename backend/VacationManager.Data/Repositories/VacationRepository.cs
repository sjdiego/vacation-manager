using Microsoft.EntityFrameworkCore;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;

namespace VacationManager.Data.Repositories;

public class VacationRepository : IVacationRepository
{
    private readonly VacationManagerDbContext _context;

    public VacationRepository(VacationManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Vacation?> GetByIdAsync(Guid id)
    {
        return await _context.Vacations
            .Include(v => v.User)
            .Include(v => v.ApprovedByUser)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Vacation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Vacations
            .Include(v => v.User)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vacation>> GetByTeamAsync(Guid teamId)
    {
        return await _context.Vacations
            .Include(v => v.User)
            .Where(v => v.User != null && v.User.TeamId == teamId)
            .OrderByDescending(v => v.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vacation>> GetPendingAsync()
    {
        return await _context.Vacations
            .Include(v => v.User)
            .Where(v => v.Status == VacationStatus.Pending)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vacation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Vacations
            .Include(v => v.User)
            .Where(v => v.StartDate <= endDate && v.EndDate >= startDate && v.Status == VacationStatus.Approved)
            .ToListAsync();
    }

    public async Task<Vacation> CreateAsync(Vacation vacation)
    {
        _context.Vacations.Add(vacation);
        await _context.SaveChangesAsync();
        return vacation;
    }

    public async Task<Vacation> UpdateAsync(Vacation vacation)
    {
        _context.Vacations.Update(vacation);
        await _context.SaveChangesAsync();
        return vacation;
    }

    public async Task DeleteAsync(Guid id)
    {
        var vacation = await GetByIdAsync(id);
        if (vacation != null)
        {
            _context.Vacations.Remove(vacation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
