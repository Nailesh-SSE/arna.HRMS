using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class LeaveValidator
{
    private readonly LeaveRepository _repository;

    public LeaveValidator(LeaveRepository repository)
    {
        _repository = repository;
    }

    // ============================
    // LEAVE TYPE
    // ============================

    public async Task<ValidationResult> ValidateLeaveTypeCreateAsync(LeaveTypeDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");
        return await ValidateLeaveTypeCommonAsync(dto);
    }

    public async Task<ValidationResult> ValidateLeaveTypeUpdateAsync(LeaveTypeDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");
        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid Leave Type ID");
        var exist = _repository.GetLeaveTypeByIdAsync(dto.Id);
        if (exist.Result == null)
            return ValidationResult.Fail("No Data Found");

        return await ValidateLeaveTypeCommonAsync(dto);
    }

    private async Task<ValidationResult> ValidateLeaveTypeCommonAsync(LeaveTypeDto dto)
    {
        var errors = new List<string>();

        if (!Enum.IsDefined(typeof(LeaveName), dto.LeaveNameId))
            errors.Add("Leave name is required");

        if (dto.MaxPerYear <= 0)
            errors.Add("Leave days must be greater than 0");

        var exists = await _repository.LeaveExistsAsync(dto.LeaveNameId, dto.Id);
        if (exists)
            errors.Add($"Leave '{dto.LeaveNameId}' already exists");
        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }

    // ============================
    // LEAVE REQUEST
    // ============================

    public async Task<ValidationResult> ValidateLeaveRequestCreateAsync(LeaveRequestDto dto)
    {
        return await ValidateLeaveRequestCommonAsync(dto);
    }

    public async Task<ValidationResult> ValidateLeaveRequestUpdateAsync(LeaveRequestDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");
        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid Leave Request ID");
        var exist = _repository.GetLeaveRequestByIdAsync(dto.Id);
        if (exist.Result == null)
            return ValidationResult.Fail("No Such Data Found");

        return await ValidateLeaveRequestCommonAsync(dto);
    }

    private Task<ValidationResult> ValidateLeaveRequestCommonAsync(LeaveRequestDto dto)
    {
        var errors = new List<string>();

        if (dto.EmployeeId <= 0)
            errors.Add("Invalid Employee Id");

        if (dto.LeaveTypeId <= 0)
            errors.Add("Invalid Leave Type Id");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            errors.Add("Reason is required");

        if (dto.StartDate == default || dto.EndDate == default)
            errors.Add("Start and End date are required");

        if (dto.StartDate.Date > dto.EndDate.Date)
            errors.Add("Start date cannot be after End date");

        if (dto.StartDate.Date < DateTime.Today.Date || dto.EndDate.Date < DateTime.Today.Date)
            errors.Add("Leave dates cannot be in the past");

        /*    if ((LeaveName)dto.LeaveTypeId != LeaveName.SickLeave)
            {
                if (dto.StartDate.Date == DateTime.Today.Date || dto.EndDate.Date == DateTime.Today.Date)
                    errors.Add("Leave dates cannot be toDays date");
            }*/

        return Task.FromResult(
            errors.Any()
                ? ValidationResult.Fail(errors.ToArray())
                : ValidationResult.Success());
    }
}
