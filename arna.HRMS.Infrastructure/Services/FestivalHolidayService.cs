using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class FestivalHolidayService : IFestivalHolidayService
{
    private readonly FestivalHolidayRepository _festivalHolidayRepository;
    private readonly IMapper _mapper;

    public FestivalHolidayService(FestivalHolidayRepository holidayRepository, IMapper mapper)
    {
        _festivalHolidayRepository = holidayRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayAsync()
    {
        var holiday = await _festivalHolidayRepository.GetFestivalHolidayAsync();
        var list = _mapper.Map<List<FestivalHolidayDto>>(holiday);

        return ServiceResult<List<FestivalHolidayDto>>.Success(list);
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayByMonthAsync(int year, int month)
    {
        if (year <= 0)
            return ServiceResult<List<FestivalHolidayDto>>.Fail("Invalid year");

        if (month < 1 || month > 12)
            return ServiceResult<List<FestivalHolidayDto>>.Fail("Invalid month");

        var holidays = await _festivalHolidayRepository.GetByMonthAsync(year, month);
        var list = _mapper.Map<List<FestivalHolidayDto>>(holidays);

        return ServiceResult<List<FestivalHolidayDto>>.Success(list);
    }

    public async Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto)
    {
        if (festivalHolidayDto == null)
            return ServiceResult<FestivalHolidayDto>.Fail("Invalid request");

        var festival = _mapper.Map<FestivalHoliday>(festivalHolidayDto);

        var createdFestivalHoliday =
            await _festivalHolidayRepository.CreateFestivalHolidayAsync(festival);

        var resultDto = _mapper.Map<FestivalHolidayDto>(createdFestivalHoliday);

        return ServiceResult<FestivalHolidayDto>.Success(resultDto, "Festival holiday created successfully");
    }

    public async Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto)
    {
        if (festivalHolidayDto == null)
            return ServiceResult<FestivalHolidayDto>.Fail("Invalid request");

        if (festivalHolidayDto.Id <= 0)
            return ServiceResult<FestivalHolidayDto>.Fail("Invalid FestivalHoliday ID");

        var festival = _mapper.Map<FestivalHoliday>(festivalHolidayDto);
        var updatedFestivalHoliday = await _festivalHolidayRepository.UpdateFestivalHolidayAsync(festival);
        var resultDto = _mapper.Map<FestivalHolidayDto>(updatedFestivalHoliday);

        return ServiceResult<FestivalHolidayDto>.Success(resultDto, "Festival holiday updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid FestivalHoliday ID");

        var deleted = await _festivalHolidayRepository.DeleteFestivalHolidayAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Festival holiday deleted successfully")
            : ServiceResult<bool>.Fail("Festival not found");
    }
}
