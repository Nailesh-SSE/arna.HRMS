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
        var mapped = _mapper.Map<List<FestivalHolidayDto>>(data);
        var response = ServiceResult<List<FestivalHolidayDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<FestivalHolidayDto?>> GetFestivalHolidayByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<FestivalHolidayDto?>.Fail("Invalid FestivalHoliday ID");
            return failId;
        }

        var entity = await _repository.GetFestivalHolidayByIdAsync(id);
        if (entity == null)
        {
            var failNotFound = ServiceResult<FestivalHolidayDto?>.Fail("Festival holiday not found");
            return failNotFound;
        }

        var mapped = _mapper.Map<FestivalHolidayDto>(entity);
        var response = ServiceResult<FestivalHolidayDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByMonthAsync(int year, int month)
    {
        var validation = _validator.ValidationByMonthAndYearAsync(year, month);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<List<FestivalHolidayDto>>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var data = await _repository.GetByMonthAsync(year, month);
        if (data == null || !data.Any())
        {
            var failNotFound = ServiceResult<List<FestivalHolidayDto>>.Fail("Festival holidays not found");
            return failNotFound;
        }

        var mapped = _mapper.Map<List<FestivalHolidayDto>>(data);
        var response = ServiceResult<List<FestivalHolidayDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var failName = ServiceResult<List<FestivalHolidayDto>>.Fail("Festival name is required");
            return failName;
        }

        var data = await _repository.GetByNameAsync(name);
        if (data == null || !data.Any())
        {
            var failNotFound = ServiceResult<List<FestivalHolidayDto>>.Fail("No Data Found");
            return failNotFound;
        }

        var mapped = _mapper.Map<List<FestivalHolidayDto>>(data);
        var response = ServiceResult<List<FestivalHolidayDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<FestivalHolidayDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<FestivalHoliday>(dto);
        var created = await _repository.CreateFestivalHolidayAsync(entity);
        var mapped = _mapper.Map<FestivalHolidayDto>(created);
        var response = ServiceResult<FestivalHolidayDto>.Success(mapped, "Festival holiday created successfully.");
        return response;
    }

    public async Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<FestivalHolidayDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<FestivalHoliday>(dto);
        var updated = await _repository.UpdateFestivalHolidayAsync(entity);
        var mapped = _mapper.Map<FestivalHolidayDto>(updated);
        var response = ServiceResult<FestivalHolidayDto>.Success(mapped, "Festival holiday updated successfully.");
        return response;
    }

    public async Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Festival holiday not found");
            return failId;
        }

        var deleted = await _repository.DeleteFestivalHolidayAsync(id);
        if (deleted)
        {
            var success = ServiceResult<bool>.Success(true, "Festival holiday deleted successfully");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("Festival holiday not found");
        return failNotFound;
    }
}
