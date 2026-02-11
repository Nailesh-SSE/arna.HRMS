using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Admin.FestivalHolidays;

public interface IFestivalHoliday
{
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayAsync();
    Task<ApiResult<FestivalHolidayViewModel>> GetFestivalHolidayByIdAsync(int id);
    Task<ApiResult<FestivalHolidayViewModel>> CreateFestivalHolidayAsync(FestivalHolidayViewModel model);
    Task<ApiResult<bool>> UpdateFestivalHolidayAsync(int id, FestivalHolidayViewModel model);
    Task<ApiResult<bool>> DeleteFestivalHolidayAsync(int id);
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByNameAsync(string name);
    Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayByMonthAsync(int year, int month);
}


public class FestivalHolidayService : IFestivalHoliday
{
    private readonly ApiClients _api;

    public FestivalHolidayService(ApiClients api)
    {
        _api = api;
    }

    public Task<ApiResult<List<FestivalHolidayViewModel>>> GetFestivalHolidayAsync()
        => _api.FestivalHoliday.GetAll();

    public Task<ApiResult<FestivalHolidayViewModel>> GetFestivalHolidayByIdAsync(int id)
        => _api.FestivalHoliday.GetById(id);

    public Task<ApiResult<FestivalHolidayViewModel>> CreateFestivalHolidayAsync(FestivalHolidayViewModel model)
        => _api.FestivalHoliday.Create(model);

    public Task<ApiResult<bool>> UpdateFestivalHolidayAsync(int id, FestivalHolidayViewModel model)
        => _api.FestivalHoliday.Update(id, model);

    public Task<ApiResult<bool>> DeleteFestivalHolidayAsync(int id)
        => _api.FestivalHoliday.Delete(id);

    public Task<ApiResult<List<FestivalHolidayViewModel>>>GetFestivalHolidayByNameAsync(string name)
        => _api.FestivalHoliday.GetByName(name);

    public Task<ApiResult<List<FestivalHolidayViewModel>>>GetFestivalHolidayByMonthAsync(int year, int month)
            => _api.FestivalHoliday.GetByMonth(year, month);

}

