using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class DepartmentValidator
{
    private readonly DepartmentRepository _repository;

    public DepartmentValidator(DepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(DepartmentDto instance)
    {
        return await ValidateCommonAsync(instance);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(DepartmentDto instance)
    {
        if (instance.Id <= 0)
            return ValidationResult.Fail("Invalid Department ID");

        return await ValidateCommonAsync(instance);
    }

    // =====================================================
    // COMMON VALIDATION
    // =====================================================
    private async Task<ValidationResult> ValidateCommonAsync(DepartmentDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        var errors = new List<string>();

        // Name
        if (string.IsNullOrWhiteSpace(instance.Name))
            errors.Add("Department Name is required");
        else if (instance.Name.Length > 100)
            errors.Add("Department Name must be at most 100 characters");
        else if (await _repository.DepartmentExistsAsync(instance.Name))
            errors.Add("Department name already exists");

        // Code
        if (string.IsNullOrWhiteSpace(instance.Code))
            errors.Add("Department Code is required");
        else if (instance.Code.Length > 50)
            errors.Add("Department Code must be at most 50 characters");

        // Description
        if (!string.IsNullOrWhiteSpace(instance.Description) && instance.Description.Length > 500)
            errors.Add("Department Description must be at most 500 characters");

        // Duplicate 
        var duplicate = await _repository.DepartmentExistsAsync(instance.Name);
        if (duplicate)
            errors.Add("Department already exists.");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }
}
