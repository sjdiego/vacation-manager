namespace VacationManager.Core.Entities;

public class Team
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public ICollection<User> Members { get; set; } = [];
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
