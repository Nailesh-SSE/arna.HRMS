using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class RoleValidator
{
    private readonly RoleRepository _repository;

    public RoleValidator(RoleRepository repository)
    {
        _repository = repository; 
    }

    public async Task<ValidationResult> ValidateCreateAsync(RoleDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");
        return await ValidateCommonAsync(instance);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(RoleDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");
        if (instance.Id <= 0)
            return ValidationResult.Fail("Invalid role ID");
        var exist = _repository.GetRoleByIdAsync(instance.Id);
        if (exist.Result == null)
            return ValidationResult.Fail("No Data Found");
        return await ValidateCommonAsync(instance);
    }

    // =====================================================
    // COMMON VALIDATION
    // =====================================================
    private async Task<ValidationResult> ValidateCommonAsync(RoleDto instance)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(instance.Name))
            errors.Add("Role name is required");
        else if (instance.Name.Length > 100)
            errors.Add("Role name must be at most 100 characters");
        else if (await _repository.RoleExistsAsync(instance.Name, instance.Id))
            errors.Add("Role name already exists");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }
}
