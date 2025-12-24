namespace VacationManager.Core.Entities;

public enum VacationType
{
    Vacation = 0,
    SickLeave = 1,
    PersonalDay = 2,
    CompensatoryTime = 3,
    Other = 4
}

public enum VacationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public class Vacation
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public User? User { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public VacationType Type { get; set; } = VacationType.Vacation;
    
    public VacationStatus Status { get; set; } = VacationStatus.Pending;
    
    public Guid? ApprovedBy { get; set; }
    
    public User? ApprovedByUser { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
