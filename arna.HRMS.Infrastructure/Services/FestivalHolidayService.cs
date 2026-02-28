using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class FestivalHolidayService : IFestivalHolidayService
{
    private readonly FestivalHolidayRepository _repository;
    private readonly IMapper _mapper;
    private readonly FestivalHolidayValidator _validator;

    public FestivalHolidayService(
        FestivalHolidayRepository repository,
        IMapper mapper,
        FestivalHolidayValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysAsync()
    {
        var data = await _repository.GetFestivalHolidaysAsync();

        return ServiceResult<List<FestivalHolidayDto>>.Success(_mapper.Map<List<FestivalHolidayDto>>(data));
    }

    public async Task<ServiceResult<FestivalHolidayDto?>> GetFestivalHolidayByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<FestivalHolidayDto?>.Fail("Invalid festival holiday ID.");

        var entity = await _repository.GetFestivalHolidayByIdAsync(id);

        if (entity == null)
            return ServiceResult<FestivalHolidayDto?>.Fail("Festival holiday not found.");

        return ServiceResult<FestivalHolidayDto?>.Success(_mapper.Map<FestivalHolidayDto>(entity));
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByMonthAsync(int year, int month)
    {
        var validation = _validator.ValidationByMonthAndYearAsync(year, month);

        if (!validation.IsValid)
            return ServiceResult<List<FestivalHolidayDto>>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var data = await _repository.GetByMonthAsync(year, month);

        return ServiceResult<List<FestivalHolidayDto>>.Success(_mapper.Map<List<FestivalHolidayDto>>(data));
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<List<FestivalHolidayDto>>.Fail("Festival name is required.");

        var data = await _repository.GetByNameAsync(name);

        return ServiceResult<List<FestivalHolidayDto>>.Success(_mapper.Map<List<FestivalHolidayDto>>(data));
    }

    public async Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<FestivalHolidayDto>
                .Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<FestivalHoliday>(dto);

        var created = await _repository.CreateFestivalHolidayAsync(entity);

        return ServiceResult<FestivalHolidayDto>.Success(_mapper.Map<FestivalHolidayDto>(created), "Festival holiday created successfully.");
    }

    public async Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<FestivalHolidayDto>
                .Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<FestivalHoliday>(dto);

        var updated = await _repository.UpdateFestivalHolidayAsync(entity);

        return ServiceResult<FestivalHolidayDto>.Success(_mapper.Map<FestivalHolidayDto>(updated), "Festival holiday updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid festival holiday ID.");

        var deleted = await _repository.DeleteFestivalHolidayAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Festival holiday deleted successfully.")
            : ServiceResult<bool>.Fail("Festival holiday not found.");
    }
}