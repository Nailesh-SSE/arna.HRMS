namespace arna.HRMS.Core.Common.ServiceResult;

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static ValidationResult Success()
        => new ValidationResult { IsValid = true };

    public static ValidationResult Fail(params string[] errors)
        => new ValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
}
