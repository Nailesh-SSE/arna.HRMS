using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IFestivalHolidayService
{
    Task<List<FestivalHolidayDto>> GetFestivalHolidayAsync();
    Task<List<FestivalHolidayDto>> GetFestivalHolidayByMonthAsync(int year, int month);
}
