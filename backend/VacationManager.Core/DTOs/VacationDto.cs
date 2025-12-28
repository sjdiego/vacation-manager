using VacationManager.Core.Entities;

namespace VacationManager.Core.DTOs;

public class VacationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VacationType Type { get; set; }
    public VacationStatus Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateVacationDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VacationType Type { get; set; } = VacationType.Vacation;
    public string? Notes { get; set; }
}

public class UpdateVacationDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public VacationType? Type { get; set; }
    public string? Notes { get; set; }
}

public class ApproveVacationDto
{
    public bool Approved { get; set; }
    public string? RejectReason { get; set; }
}
