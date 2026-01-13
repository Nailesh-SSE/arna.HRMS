using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
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

    public async Task<List<FestivalHolidayDto>> GetFestivalHolidayAsync()
    {
        var holiday = await _festivalHolidayRepository.GetFestivalHolidayAsync();
        return _mapper.Map<List<FestivalHolidayDto>>(holiday);
    }
    public async Task<List<FestivalHolidayDto>> GetFestivalHolidayByMonthAsync(int year, int month)
    {
        var holidays = await _festivalHolidayRepository.GetByMonthAsync(year, month);
        return _mapper.Map<List<FestivalHolidayDto>>(holidays);
    }


}
