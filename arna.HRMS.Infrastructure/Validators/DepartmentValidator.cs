using arna.HRMS.Core.Common.Results;
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
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        return await ValidateCommonAsync(instance);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(DepartmentDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        if (instance.Id <= 0)
            return ValidationResult.Fail("Invalid Department ID");

        var exist = await _repository.GetDepartmentByIdAsync(instance.Id);
        if (exist == null)
            return ValidationResult.Fail("Data not found");

        return await ValidateCommonAsync(instance);
    }

    // =====================================================
    // COMMON VALIDATION
    // =====================================================
    private async Task<ValidationResult> ValidateCommonAsync(DepartmentDto instance)
    {
        var errors = new List<string>();

        // Name
        if (string.IsNullOrWhiteSpace(instance.Name))
            errors.Add("Department Name is required");
        else if (instance.Name.Length > 100)
            errors.Add("Department Name must be at most 100 characters");

        // Code
        if (string.IsNullOrWhiteSpace(instance.Code))
            errors.Add("Department Code is required");
        else if (instance.Code.Length > 50)
            errors.Add("Department Code must be at most 50 characters");

        // Description (optional)
        if (!string.IsNullOrWhiteSpace(instance.Description) && instance.Description.Length > 500)
            errors.Add("Department Description must be at most 500 characters");

        // Duplicate 
        var duplicate = await _repository.DepartmentExistsAsync(instance.Name, instance.Id);
        if (duplicate)
            errors.Add("Department already exists.");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }
}
