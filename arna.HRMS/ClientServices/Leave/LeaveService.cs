using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.ClientServices.Leave;

public interface ILeaveService
{
    //Leave Master Methods
    Task<ApiResult<List<LeaveMasterViewModel>>> GetLeaveMasterAsync();
    Task<ApiResult<LeaveMasterViewModel>> GetLeaveMasterByIdAsync(int id);
    Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMasterAsync(LeaveMasterViewModel leaveMasterViewModel);
    Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel viewModel); 

    //Leave Request Methods
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync();
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByStatusAsync(Status status);
    Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int Id);
    Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel leaveRequestViewModel);
    Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel viewModel); 
    Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status);
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByEmployee(int employeeid);
    Task<ApiResult<bool>> UpadteLeaverequestStatusCancle(int leaveRequestId, int employeeid);

    //Leave Balance Methods
    Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync();
    Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel leaveBalanceViewModel);
    Task<ApiResult<List<EmployeeLeaveBalanceViewModel?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
}
public class LeaveService : ILeaveService
{
    private readonly ApiClients.LeaveApi _leaveApi;

    public LeaveService(ApiClients api)
    {
        _leaveApi = api.Leave;
    }

    //Leave Master Methods
    public async Task<ApiResult<List<LeaveMasterViewModel>>> GetLeaveMasterAsync()
        => await _leaveApi.GetAllLeaveMaster();

    public async Task<ApiResult<LeaveMasterViewModel>> GetLeaveMasterByIdAsync(int id)
        => await _leaveApi.GetLeaveMasterById(id);

    public async Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMasterAsync(LeaveMasterViewModel leaveMasterViewModel)
        => await _leaveApi.CreateLeaveMaster(leaveMasterViewModel);

    public async Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id)
        => await _leaveApi.DeleteLeaveMasterAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel viewModel) 
        => await _leaveApi.UpdateLeaveMasterAsync(id, viewModel);

    //Leave Request Methods
    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync()
        => await _leaveApi.GetAllLeaveRequest();

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByStatusAsync(Status status)
        => await _leaveApi.GetRequestByStatusAsync(status);

    public async Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int Id)
        => await _leaveApi.GetLeaveRequestById(Id);

    public async Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel leaveRequestViewModel)
        => await _leaveApi.CreateLeaveRequest(leaveRequestViewModel);

    public async Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
        => await _leaveApi.DeleteLeaveRequestAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel viewModel)
        => await _leaveApi.UpdateLeaveRequestAsync(id, viewModel);
     
    public async Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status)
        => await _leaveApi.UpdateStatusLeaveAsync(leaveRequestId, status);

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByEmployee(int employeeid)
        => await _leaveApi.GetLeaveRequestByEmployee(employeeid);

    public async Task<ApiResult<bool>> UpadteLeaverequestStatusCancle(int leaveRequestId, int employeeid)
        => await _leaveApi.UpadteLeaverequestStatusCancle(leaveRequestId, employeeid);
    //Leave Balance Methods
    public async Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync()
        => await _leaveApi.GetLeaveBalanceAsync();

    public async Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id)
        => await _leaveApi.DeleteLeaveBalanceAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel leaveBalanceViewModel)
        => await _leaveApi.UpdateLeaveBalanceAsync(id, leaveBalanceViewModel);

    public async Task<ApiResult<List<EmployeeLeaveBalanceViewModel?>?>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
        => await _leaveApi.GetLeaveBalanceByEmployeeIdAsync(employeeId);

}
