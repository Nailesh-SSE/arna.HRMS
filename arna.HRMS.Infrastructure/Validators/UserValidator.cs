using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;
using System.Text.RegularExpressions;
using EmailAddressAttribute = System.ComponentModel.DataAnnotations.EmailAddressAttribute;
using PhoneAttribute = System.ComponentModel.DataAnnotations.PhoneAttribute;

namespace arna.HRMS.Infrastructure.Validators;

public class UserValidator
{
    private readonly UserRepository _repository;

    public UserValidator(UserRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(UserDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        return await ValidateCommonAsync(instance);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(UserDto instance)
    {
        if (instance == null)
            return ValidationResult.Fail("Invalid request");

        if (instance.Id <= 0)
            return ValidationResult.Fail("Invalid User ID");
        var exist = _repository.GetUserByIdAsync(instance.Id);
        if (exist == null)
            return ValidationResult.Fail("No Data Found");

        return await ValidateCommonAsync(instance);
    }

    public ValidationResult ValidateChangePasswordAsync(int id, string newPassword)
    {
        if (id <= 0)
            return ValidationResult.Fail("Invalid User ID");
        var exist = _repository.GetUserByIdAsync(id);
        if (exist.Result == null)
            return ValidationResult.Fail("No Data Found");
        else if (string.IsNullOrWhiteSpace(newPassword))
            return ValidationResult.Fail("Password is required");

        else if (newPassword.Length < 6 || newPassword.Length > 100)
            return ValidationResult.Fail("Password must be between 6 and 100 characters");

        return ValidationResult.Success(); 
    }

    private async Task<ValidationResult> ValidateCommonAsync(UserDto instance)
    {
        var errors = new List<string>();

        // Username
        if (string.IsNullOrWhiteSpace(instance.Username))
            errors.Add("Username is required");
        else if (instance.Username.Length < 3 || instance.Username.Length > 100)
            errors.Add("Username must be between 3 and 100 characters");

        // Email
        if (string.IsNullOrWhiteSpace(instance.Email))
            errors.Add("Email is required");
        else if (instance.Email.Length > 100)
            errors.Add("Email must be at most 100 characters");
        else if (!new EmailAddressAttribute().IsValid(instance.Email))
            errors.Add("Invalid email format");

        // Password (create OR when provided)
        if (string.IsNullOrWhiteSpace(instance.Password))
            errors.Add("Password is required");
        else if (instance.Password.Length < 6 || instance.Password.Length > 100)
            errors.Add("Password must be between 6 and 100 characters");

        // First Name
        if (string.IsNullOrWhiteSpace(instance.FirstName))
            errors.Add("First name is required");
        else if (instance.FirstName.Length > 50)
            errors.Add("First name must be at most 50 characters");

        // Last Name
        if (string.IsNullOrWhiteSpace(instance.LastName))
            errors.Add("Last name is required");
        else if (instance.LastName.Length > 50)
            errors.Add("Last name must be at most 50 characters");

        // Role
        if (instance.RoleId <= 0)
            errors.Add("Role is required");

        // Phone
        if (string.IsNullOrWhiteSpace(instance.PhoneNumber))
            errors.Add("Phone number is required");
        else if (!new PhoneAttribute().IsValid(instance.PhoneNumber))
            errors.Add("Invalid phone number format");
        else if (Regex.Replace(instance.PhoneNumber, @"\D", "").Length != 10)
            errors.Add("Phone number must be exactly 10 digits");

        // Duplicate 
        if (await _repository.UserExistsAsync(instance.Email, instance.PhoneNumber ?? string.Empty))
            errors.Add("Email or Phone Number already exists");

        return errors.Any()
                ? ValidationResult.Fail(errors.ToArray())
                : ValidationResult.Success();
    }
}
