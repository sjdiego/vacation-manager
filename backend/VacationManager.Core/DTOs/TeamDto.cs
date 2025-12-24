namespace VacationManager.Core.DTOs;

public class TeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTeamDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
