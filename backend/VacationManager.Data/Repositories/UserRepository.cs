using Microsoft.EntityFrameworkCore;
using VacationManager.Core.Entities;
using VacationManager.Core.Interfaces;

namespace VacationManager.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly VacationManagerDbContext _context;

    public UserRepository(VacationManagerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEntraIdAsync(string entraId)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.EntraId == entraId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Team)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByTeamAsync(Guid teamId)
    {
        return await _context.Users
            .Where(u => u.TeamId == teamId)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
