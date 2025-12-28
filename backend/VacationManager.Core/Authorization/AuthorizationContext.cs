using VacationManager.Core.Entities;

namespace VacationManager.Core.Authorization;

/// <summary>
/// Context containing information needed for authorization checks
/// </summary>
public class AuthorizationContext
{
    public User User { get; set; } = null!;
    public string Operation { get; set; } = string.Empty;
    public object? Resource { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
