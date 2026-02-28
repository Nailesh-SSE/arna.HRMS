using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Leave;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface ILeaveService
{
    // ===================== LEAVE TYPE =====================

    Task<ApiResult<List<LeaveTypeViewModel>>> GetLeaveTypesAsync();
    Task<ApiResult<LeaveTypeViewModel>> GetLeaveTypeByIdAsync(int id);
    Task<ApiResult<LeaveTypeViewModel>> CreateLeaveTypeAsync(LeaveTypeViewModel model);
    Task<ApiResult<bool>> UpdateLeaveTypeAsync(int id, LeaveTypeViewModel model);
    Task<ApiResult<bool>> DeleteLeaveTypeAsync(int id);

    // ===================== LEAVE REQUEST =====================

    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsAsync();
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsByFilterAsync(Status? status, int? empId);
    Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int id);
    Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel model);
    Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel model);
    Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveStatusAsync(int leaveRequestId, Status status);
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsByEmployeeAsync(int employeeId);
    Task<ApiResult<bool>> CancelLeaveRequestAsync(int leaveRequestId, int employeeId);
}

public class LeaveService : ILeaveService
{
    private readonly ApiClients.LeaveApi _leaveApi;

    public LeaveService(ApiClients api)
    {
        _leaveApi = api.Leave;
    }

    // ===================== LEAVE TYPE =====================

    public async Task<ApiResult<List<LeaveTypeViewModel>>> GetLeaveTypesAsync()
    {
        return await _leaveApi.LeaveTypes.GetAllAsync();
    }

    public async Task<ApiResult<LeaveTypeViewModel>> GetLeaveTypeByIdAsync(int id)
    {
        return await _leaveApi.LeaveTypes.GetByIdAsync(id);
    }

    public async Task<ApiResult<LeaveTypeViewModel>> CreateLeaveTypeAsync(LeaveTypeViewModel model)
    {
        return await _leaveApi.LeaveTypes.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateLeaveTypeAsync(int id, LeaveTypeViewModel model)
    {
        return await _leaveApi.LeaveTypes.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteLeaveTypeAsync(int id)
    {
        return await _leaveApi.LeaveTypes.DeleteAsync(id);
    }

    // ===================== LEAVE REQUEST =====================

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsAsync()
    {
        return await _leaveApi.LeaveRequests.GetAllAsync();
    }

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsByFilterAsync(Status? status, int? empId)
    {
        return await _leaveApi.GetByFilterAsync(status, empId);
    }

    public async Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int id)
    {
        return await _leaveApi.LeaveRequests.GetByIdAsync(id);
    }

    public async Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel model)
    {
        return await _leaveApi.LeaveRequests.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel model)
    {
        return await _leaveApi.LeaveRequests.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
    {
        return await _leaveApi.LeaveRequests.DeleteAsync(id);
    }

    public async Task<ApiResult<bool>> UpdateLeaveStatusAsync(int leaveRequestId, Status status)
    {
        return await _leaveApi.UpdateStatusAsync(leaveRequestId, status);
    }

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestsByEmployeeAsync(int employeeId)
    {
        return await _leaveApi.GetByFilterAsync(null, employeeId);
    }

    public async Task<ApiResult<bool>> CancelLeaveRequestAsync(int leaveRequestId, int employeeId)
    {
        return await _leaveApi.CancelAsync(leaveRequestId, employeeId);
    }
}