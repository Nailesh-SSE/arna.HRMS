using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services.Dashboard;

public interface IDashboardService
{
    Task<ApiResult<DashboardViewModel>> GetAllAsync(Status? status,int? employeeId);
}
public class DashboardService : IDashboardService
{

    private readonly ApiClients.DashboardApi _dashboardApi;

    public DashboardService(ApiClients api)
    {
        _dashboardApi = api.Dashboard;
    }

    public async Task<ApiResult<DashboardViewModel>> GetAllAsync(Status? status, int? employeeId)
    {
        return await _dashboardApi.GetDashboardAsync(status, employeeId);
    }
}
