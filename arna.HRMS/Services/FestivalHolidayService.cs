using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IFestivalHolidayService
{
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayAsync();
    Task<ApiResult<FestivalHolidayViewModel>> GetFestivalHolidayByIdAsync(int id);
    Task<ApiResult<FestivalHolidayViewModel>> CreateFestivalHolidayAsync(FestivalHolidayViewModel model);
    Task<ApiResult<bool>> UpdateFestivalHolidayAsync(int id, FestivalHolidayViewModel model);
    Task<ApiResult<bool>> DeleteFestivalHolidayAsync(int id);
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByNameAsync(string name);
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByMonthAsync(int year, int month);
}

public class FestivalHolidayService : IFestivalHolidayService
{
    private readonly ApiClients.FestivalHolidayApi _festivalHoliday;

    public FestivalHolidayService(ApiClients api)
    {
        _festivalHoliday = api.FestivalHoliday;
    }

    public async Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayAsync()
    {
        return await _festivalHoliday.GetAllAsync();
    }

    public async Task<ApiResult<FestivalHolidayViewModel>> GetFestivalHolidayByIdAsync(int id)
    {
        return await _festivalHoliday.GetByIdAsync(id);
    }

    public async Task<ApiResult<FestivalHolidayViewModel>> CreateFestivalHolidayAsync(FestivalHolidayViewModel model)
    {
        return await _festivalHoliday.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateFestivalHolidayAsync(int id, FestivalHolidayViewModel model)
    {
        return await _festivalHoliday.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteFestivalHolidayAsync(int id)
    {
        return await _festivalHoliday.DeleteAsync(id);
    }

    public async Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByNameAsync(string name)
    {
        return await _festivalHoliday.GetByNameAsync(name);
    }

    public async Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByMonthAsync(int year, int month)
    {
        return await _festivalHoliday.GetByMonthAsync(year, month);
    }
}