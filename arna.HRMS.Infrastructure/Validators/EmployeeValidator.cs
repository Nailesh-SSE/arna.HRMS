using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;
using System.Text.RegularExpressions;
using EmailAddressAttribute = System.ComponentModel.DataAnnotations.EmailAddressAttribute;
using PhoneAttribute = System.ComponentModel.DataAnnotations.PhoneAttribute;

namespace arna.HRMS.Infrastructure.Validators;

public class EmployeeValidator
{
    private readonly EmployeeRepository _repository;

    public EmployeeValidator(EmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(EmployeeDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");

        return await ValidateCommonFields(dto);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(EmployeeDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");

        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid Employee ID");

        var exist = await _repository.GetEmployeeByIdAsync(dto.Id);
        if (exist == null)
            return ValidationResult.Fail("Employee not found");

        return await ValidateCommonFields(dto);
    }

    private async Task<ValidationResult> ValidateCommonFields(EmployeeDto instance)
    {
        var errors = new List<string>();

        ValidateNames(instance, errors);
        ValidateEmailsAndPhone(instance, errors);
        ValidateDatesAndRole(instance, errors);
        await ValidateDuplicatesAsync(instance, errors);

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }

    private static void ValidateNames(EmployeeDto instance, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(instance.FirstName))
            errors.Add("FirstName is required");
        else if (instance.FirstName.Length > 50)
            errors.Add("FirstName cannot exceed 50 characters");

        if (string.IsNullOrWhiteSpace(instance.LastName))
            errors.Add("LastName is required");
        else if (instance.LastName.Length > 50)
            errors.Add("LastName cannot exceed 50 characters");
    }

    private static void ValidateEmailsAndPhone(EmployeeDto instance, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(instance.Email))
            errors.Add("Email is required");
        else if (!new EmailAddressAttribute().IsValid(instance.Email))
            errors.Add("Invalid Email format");
        else if (!string.IsNullOrWhiteSpace(instance.OfficeEmail) && !new EmailAddressAttribute().IsValid(instance.OfficeEmail))
            errors.Add("Invalid OfficeEmail format");

        if (string.IsNullOrWhiteSpace(instance.PhoneNumber))
            errors.Add("Phone number is required.");
        else if (!new PhoneAttribute().IsValid(instance.PhoneNumber))
            errors.Add("Invalid phone number.");
        else if (Regex.Replace(instance.PhoneNumber, @"\D", "").Length != 10)
            errors.Add("Phone number must be exactly 10 digits.");
    }

    private static void ValidateDatesAndRole(EmployeeDto instance, List<string> errors)
    {
        if (instance.DateOfBirth == default)
            errors.Add("DateOfBirth is required");
        else if (instance.DateOfBirth > DateTime.Now)
            errors.Add("DateOfBirth cannot be in the future");

        if (instance.HireDate == default)
            errors.Add("HireDate is required");
        else if (instance.HireDate < instance.DateOfBirth)
            errors.Add("HireDate cannot be before DateOfBirth");
        else if (instance.HireDate > DateTime.Now)
            errors.Add("HireDate cannot be in the future");

        if (instance.DepartmentId is null or <= 0)
            errors.Add("Department is required");

        if (string.IsNullOrWhiteSpace(instance.Position))
            errors.Add("Position is required");
        else if (instance.Position.Length > 100)
            errors.Add("Position cannot exceed 100 characters");

        if (instance.Salary <= 0)
            errors.Add("Salary must be greater than 0");
    }

    private async Task ValidateDuplicatesAsync(EmployeeDto instance, List<string> errors)
    {
        var duplicate = await _repository.EmployeeExistsAsync(instance.Email, instance.PhoneNumber, instance.OfficeEmail, instance.Id);
        if (duplicate)
            errors.Add("Email or Phone Number already exists");
    }
}
