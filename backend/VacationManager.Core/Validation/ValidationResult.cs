namespace VacationManager.Core.Validation;

/// <summary>
/// Represents the result of a validation rule execution
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null, string? errorCode = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static ValidationResult Success() => new(true);

    public static ValidationResult Failure(string errorMessage, string? errorCode = null) 
        => new(false, errorMessage, errorCode);
}
