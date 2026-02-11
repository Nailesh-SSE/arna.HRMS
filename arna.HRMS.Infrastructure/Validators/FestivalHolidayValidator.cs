using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Validators;

public class FestivalHolidayValidator
{
    private readonly FestivalHolidayRepository _repository;

    public FestivalHolidayValidator(FestivalHolidayRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(FestivalHolidayDto dto)
    {
        return await ValidateCommonAsync(dto);
    }

    public async Task<ValidationResult> ValidateUpdateAsync(FestivalHolidayDto dto)
    {
        if (dto.Id <= 0)
            return ValidationResult.Fail("Invalid FestivalHoliday ID");

        return await ValidateCommonAsync(dto);
    }

    public ValidationResult ValidationByMonthAndYearAsync(int year, int month)
    {
        if (year <= 0)
            return ValidationResult.Fail("Invalid year");

        if (month < 1 || month > 12)
            return ValidationResult.Fail("Invalid month");

        return ValidationResult.Success();
    }

    private async Task<ValidationResult> ValidateCommonAsync(FestivalHolidayDto dto)
    {
        if (dto == null)
            return ValidationResult.Fail("Invalid request");

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.FestivalName))
            errors.Add("Festival name is required");

        if (dto.Date == default)
            errors.Add("Festival date is required");
        else if (dto.Date.Date < DateTime.Today)
            errors.Add("Festival date cannot be in the past");

        var duplicates = await _repository.GetByNameAndDateAsync(dto.FestivalName, dto.Date);
        if (duplicates == null)
            return ValidationResult.Fail("Error checking for duplicates");

        return errors.Any()
            ? ValidationResult.Fail(errors.ToArray())
            : ValidationResult.Success();
    }
}
