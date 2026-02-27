using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class AttendanceValidator
{
    private readonly AttendanceRepository _repository;

    public AttendanceValidator(AttendanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(AttendanceDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");

        var errors = new List<string>();

        if (dto.EmployeeId <= 0)
            errors.Add("EmployeeId is required");

        if (dto.Date == default)
            errors.Add("Date is required");

        if (string.IsNullOrWhiteSpace(dto.Notes))
            errors.Add("Note is Required.");

        if (string.IsNullOrEmpty(dto.Device))
            errors.Add("Device are required");

        if (dto.WorkingHours.HasValue && dto.WorkingHours.Value < TimeSpan.Zero)
            errors.Add("Working hours must be greater than or equal to zero");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }

    public async Task<ValidationResult> ValidateUpdateAsync(AttendanceDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");

        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid Attendance ID");

        var exist = await _repository.GetAttendanceByIdAsync(dto.Id);
        if (exist == null)
            return ValidationResult.Fail("Attendance not found");

        return await ValidateCreateAsync(dto);
    }
}
