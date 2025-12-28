namespace VacationManager.Core.Authorization;

/// <summary>
/// Represents the result of an authorization check
/// </summary>
public class AuthorizationResult
{
    public bool IsAuthorized { get; private set; }
    public string? FailureReason { get; private set; }
    public string? FailureCode { get; private set; }

    private AuthorizationResult(bool isAuthorized, string? failureReason = null, string? failureCode = null)
    {
        IsAuthorized = isAuthorized;
        FailureReason = failureReason;
        FailureCode = failureCode;
    }

    public static AuthorizationResult Success() => new(true);

    public static AuthorizationResult Failure(string failureReason, string? failureCode = null)
        => new(false, failureReason, failureCode);
}
