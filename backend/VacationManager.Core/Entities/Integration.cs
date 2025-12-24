namespace VacationManager.Core.Entities;

public enum IntegrationType
{
    Slack = 0,
    Outlook = 1,
    Confluence = 2,
    GoogleCalendar = 3
}

public class Integration
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public User? User { get; set; }
    
    public IntegrationType Type { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public required string Config { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
