using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IFestivalHolidayService
{
    Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysAsync();
    Task<ServiceResult<FestivalHolidayDto?>> GetFestivalHolidayByIdAsync(int id);
    Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByNameAsync(string name);
    Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidaysByMonthAsync(int year, int month);
    Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto dto);
    Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto dto);
    Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id);
}