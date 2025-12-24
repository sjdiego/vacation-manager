using VacationManager.Core.Entities;

namespace VacationManager.Core.Interfaces;

public interface IVacationRepository
{
    Task<Vacation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Vacation>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Vacation>> GetByTeamAsync(Guid teamId);
    Task<IEnumerable<Vacation>> GetPendingAsync();
    Task<IEnumerable<Vacation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Vacation> CreateAsync(Vacation vacation);
    Task<Vacation> UpdateAsync(Vacation vacation);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEntraIdAsync(string entraId);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByTeamAsync(Guid teamId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id);
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team> CreateAsync(Team team);
    Task<Team> UpdateAsync(Team team);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
