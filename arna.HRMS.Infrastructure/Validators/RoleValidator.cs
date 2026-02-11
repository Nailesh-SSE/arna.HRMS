using arna.HRMS.Core.Common.ServiceResult;
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
        return await ValidateCommonAsync(instance);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(RoleDto instance)
    {
        if(instance.Id <= 0)
            return ValidationResult.Fail("Invalid role ID");

        return await ValidateCommonAsync(instance);
    }

    // =====================================================
    // COMMON VALIDATION
    // =====================================================
    private async Task<ValidationResult> ValidateCommonAsync(RoleDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(instance.Name))
            errors.Add("Role name is required");
        else if (instance.Name.Length > 100)
            errors.Add("Role name must be at most 100 characters");
        else if (await _repository.RoleExistsAsync(instance.Name))
            errors.Add("Role name already exists");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }
}
