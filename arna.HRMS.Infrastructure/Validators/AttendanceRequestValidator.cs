using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class AttendanceRequestValidator
{
    private readonly AttendanceRequestRepository _repository;

    public AttendanceRequestValidator(AttendanceRequestRepository repository)
    {
        _repository = repository;
    }

    // =============================
    // CREATE
    // =============================
    public async Task<ValidationResult> ValidateCreateAsync(AttendanceRequestDto dto)
    {
        return await ValidateCommonAsync(dto);
    }

    // =============================
    // UPDATE
    // =============================
    public async Task<ValidationResult> ValidateUpdateAsync(AttendanceRequestDto dto)
    {
        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid Attendance Request ID");

        return await ValidateCommonAsync(dto);
    }

    // =============================
    // STATUS UPDATE
    // =============================
    public async Task<ValidationResult> ValidateStatusUpdateAsync(int id, Status status, int approvedBy)
    {
        if (id <= 0)
            return ValidationResult.Fail("Invalid AttendanceRequest ID");

        var exist = await _repository.GetAttendanceRequestByIdAsync(id);
        if (exist == null)
            return ValidationResult.Fail("Invalid Attendance Request ID");

        if (status == Status.Cancelled)
            return ValidationResult.Fail("Not Allow to Cancel");

        if (status == Status.Pending)
            return ValidationResult.Fail("Not Allow to Pending");

        if (approvedBy <= 0)
            return ValidationResult.Fail("Invalid Approved ID");

        return ValidationResult.Success();
    }

    // =============================
    // COMMON FIELD VALIDATION
    // =============================
    private Task<ValidationResult> ValidateCommonAsync(AttendanceRequestDto dto)
    {
        if (dto == null)
            return Task.FromResult(ValidationResult.Fail("Invalid request"));

        var errors = new List<string>();

        if (dto.EmployeeId <= 0)
            errors.Add("EmployeeId is required");

        if (dto.FromDate == null)
            errors.Add("From Date is required");

        if (dto.ToDate == null)
            errors.Add("To Date is required");

        if (dto.FromDate != null && dto.ToDate != null)
        {
            if (dto.FromDate.Value.Date >= DateTime.Now.Date)
                errors.Add("Future or current From Date is not allowed");

            if (dto.ToDate.Value.Date >= DateTime.Now.Date)
                errors.Add("Future or current To Date is not allowed");

            if (dto.FromDate > dto.ToDate)
                errors.Add("From Date cannot be greater than To Date");
        }

        if (dto.ClockIn == null)
            errors.Add("ClockIn is required");

        if (dto.ClockOut == null)
            errors.Add("ClockOut is required");

        if (dto.ClockIn != null && dto.ClockOut != null)
        {
            if (dto.ClockOut <= dto.ClockIn)
                errors.Add("ClockOut must be greater than ClockIn");
        }

        if (dto.LocationId == null)
            errors.Add("Location is required");

        if (dto.ReasonTypeId == null)
            errors.Add("Reason is required");

        if (dto.BreakDuration == null)
            errors.Add("Break duration is required");

        if (string.IsNullOrWhiteSpace(dto.Description) && dto.Description?.Length > 500)
            errors.Add("Description cannot exceed 500 characters");

        // Clock times should fall within the from/to dates when provided
        if (dto.ClockIn != null && dto.FromDate != null && dto.ToDate != null)
        {
            var cinDate = dto.ClockIn.Value.Date;
            if (cinDate < dto.FromDate.Value.Date || cinDate > dto.ToDate.Value.Date)
                errors.Add("ClockIn date must be within FromDate and ToDate range");
        }

        if (dto.ClockOut != null && dto.FromDate != null && dto.ToDate != null)
        {
            var coutDate = dto.ClockOut.Value.Date;
            if (coutDate < dto.FromDate.Value.Date || coutDate > dto.ToDate.Value.Date)
                errors.Add("ClockOut date must be within FromDate and ToDate range");
        }

        // Break duration and total hours basic sanity checks
        if (dto.BreakDuration != null && dto.TotalHours < dto.BreakDuration)
            errors.Add("Break duration cannot be greater than total hours");

        return errors.Any()
                ? Task.FromResult(ValidationResult.Fail(errors.ToArray()))
                : Task.FromResult(ValidationResult.Success());
    }
}
