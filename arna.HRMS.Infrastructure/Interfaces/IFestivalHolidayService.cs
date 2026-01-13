using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IFestivalHolidayService
{
    Task<List<FestivalHolidayDto>> GetFestivalHolidayAsync();
    Task<List<FestivalHolidayDto>> GetFestivalHolidayByMonthAsync(int year, int month);
    Task<FestivalHolidayDto> CreateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto);
    Task<bool> DeleteFestivalHolidayAsync(int id);
    Task<FestivalHolidayDto> UpdateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto);
}
