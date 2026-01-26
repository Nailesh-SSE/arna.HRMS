using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IFestivalHolidayService
{
    Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayAsync();
    Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayByMonthAsync(int year, int month);
    Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto);
    Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto festivalHolidayDto);
    Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id);
}
