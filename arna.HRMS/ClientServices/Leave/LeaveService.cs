using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.Enums;

namespace arna.HRMS.ClientServices.Leave;

public interface ILeaveService
{
    //Leave Master Methods
    Task<ApiResult<List<LeaveMasterDto>>> GetLeaveMasterAsync();
    Task<ApiResult<LeaveMasterDto>> GetLeaveMasterByIdAsync(int id);
    Task<ApiResult<LeaveMasterDto>> CreateLeaveMasterAsync(LeaveMasterDto leaveMasterDto);
    Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterDto dto);

    //Leave Request Methods
    Task<ApiResult<List<LeaveRequestDto>>> GetLeaveRequestAsync();
    Task<ApiResult<List<LeaveRequestDto>>> GetPandingLeaveRequestAsync();
    Task<ApiResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int Id);
    Task<ApiResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestDto Dto);
    Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, CommonStatus status);

    //Leave Balance Methods
    Task<ApiResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync();
    Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceDto leaveBalanceDto);
    Task<ApiResult<List<EmployeeLeaveBalanceDto?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
}
public class LeaveService : ILeaveService
{
    private readonly ApiClients.LeaveApi _leaveApi;

    public LeaveService(ApiClients api)
    {
        _leaveApi = api.Leave;
    }

    //Leave Master Methods
    public async Task<ApiResult<List<LeaveMasterDto>>> GetLeaveMasterAsync()
        => await _leaveApi.GetAllLeaveMaster();

    public async Task<ApiResult<LeaveMasterDto>> GetLeaveMasterByIdAsync(int id)
        => await _leaveApi.GetLeaveMasterById(id);

    public async Task<ApiResult<LeaveMasterDto>> CreateLeaveMasterAsync(LeaveMasterDto leaveMasterDto)
        => await _leaveApi.CreateLeaveMaster(leaveMasterDto);

    public async Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id)
        => await _leaveApi.DeleteLeaveMasterAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterDto dto)
        => await _leaveApi.UpdateLeaveMasterAsync(id, dto);

    //Leave Request Methods
    public async Task<ApiResult<List<LeaveRequestDto>>> GetLeaveRequestAsync()
        => await _leaveApi.GetAllLeaveRequest();

    public async Task<ApiResult<List<LeaveRequestDto>>> GetPandingLeaveRequestAsync()
        => await _leaveApi.GetPandingLeaveRequestAsync();

    public async Task<ApiResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int Id)
        => await _leaveApi.GetLeaveRequestById(Id);

    public async Task<ApiResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto)
        => await _leaveApi.CreateLeaveRequest(leaveRequestDto);

    public async Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
        => await _leaveApi.DeleteLeaveRequestAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestDto Dto)
        => await _leaveApi.UpdateLeaveRequestAsync(id, Dto);

    public async Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, CommonStatus status)
        => await _leaveApi.UpdateStatusLeaveAsync(leaveRequestId, status);

    //Leave Balance Methods
    public async Task<ApiResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync()
        => await _leaveApi.GetLeaveBalanceAsync();

    public async Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id)
        => await _leaveApi.DeleteLeaveBalanceAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceDto leaveBalanceDto)
        => await _leaveApi.UpdateLeaveBalanceAsync(id, leaveBalanceDto);

    public async Task<ApiResult<List<EmployeeLeaveBalanceDto?>?>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
        => await _leaveApi.GetLeaveBalanceByEmployeeIdAsync(employeeId);

}
