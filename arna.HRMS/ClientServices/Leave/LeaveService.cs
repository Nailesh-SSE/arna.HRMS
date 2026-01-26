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
    Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMasterAsync(LeaveMasterViewModel leaveMasterDto);
    Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel dto);

    //Leave Request Methods
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync();
    Task<ApiResult<List<LeaveRequestViewModel>>> GetPandingLeaveRequestAsync();
    Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int Id);
    Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel leaveRequestDto);
    Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel Dto);
    Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status);

    //Leave Balance Methods
    Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync();
    Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel leaveBalanceDto);
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

    public async Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMasterAsync(LeaveMasterViewModel leaveMasterDto)
        => await _leaveApi.CreateLeaveMaster(leaveMasterDto);

    public async Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id)
        => await _leaveApi.DeleteLeaveMasterAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel dto)
        => await _leaveApi.UpdateLeaveMasterAsync(id, dto);

    //Leave Request Methods
    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync()
        => await _leaveApi.GetAllLeaveRequest();

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetPandingLeaveRequestAsync()
        => await _leaveApi.GetPandingLeaveRequestAsync();

    public async Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int Id)
        => await _leaveApi.GetLeaveRequestById(Id);

    public async Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel leaveRequestDto)
        => await _leaveApi.CreateLeaveRequest(leaveRequestDto);

    public async Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
        => await _leaveApi.DeleteLeaveRequestAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel Dto)
        => await _leaveApi.UpdateLeaveRequestAsync(id, Dto);

    public async Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status)
        => await _leaveApi.UpdateStatusLeaveAsync(leaveRequestId, status);

    //Leave Balance Methods
    public async Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync()
        => await _leaveApi.GetLeaveBalanceAsync();

    public async Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id)
        => await _leaveApi.DeleteLeaveBalanceAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel leaveBalanceDto)
        => await _leaveApi.UpdateLeaveBalanceAsync(id, leaveBalanceDto);

    public async Task<ApiResult<List<EmployeeLeaveBalanceViewModel?>?>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
        => await _leaveApi.GetLeaveBalanceByEmployeeIdAsync(employeeId);

}
