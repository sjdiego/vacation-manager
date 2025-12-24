namespace VacationManager.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public required string EntraId { get; set; }
    
    public required string Email { get; set; }
    
    public required string DisplayName { get; set; }
    
    public string? Department { get; set; }
    
    public Guid? TeamId { get; set; }
    
    public Team? Team { get; set; }
    
    public bool IsManager { get; set; } = false;
    
    public ICollection<Vacation> Vacations { get; set; } = [];
    
    public ICollection<Vacation> ApprovedVacations { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
