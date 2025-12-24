namespace VacationManager.Core.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public Guid? TeamId { get; set; }
    public bool IsManager { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? Department { get; set; }
    public Guid? TeamId { get; set; }
}

public class UpdateUserDto
{
    public string? DisplayName { get; set; }
    public string? Department { get; set; }
    public Guid? TeamId { get; set; }
}
