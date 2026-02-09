using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.ClientServices.Leave;

public interface ILeaveService
{
    //Leave Type Methods
    Task<ApiResult<List<LeaveTypeViewModel>>> GetLeaveTypeAsync();
    Task<ApiResult<LeaveTypeViewModel>> GetLeaveTypeByIdAsync(int id);
    Task<ApiResult<LeaveTypeViewModel>> CreateLeaveTypeAsync(LeaveTypeViewModel leaveTypeViewModel);
    Task<ApiResult<bool>> DeleteLeaveTypeAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveTypeAsync(int id, LeaveTypeViewModel viewModel); 

    //Leave Request Methods
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync();
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByFilterAsync(Status? status, int? empId);
    Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestByIdAsync(int Id);
    Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequestAsync(LeaveRequestViewModel leaveRequestViewModel);
    Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel viewModel); 
    Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status);
    Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByEmployee(int employeeid);
    Task<ApiResult<bool>> UpadteLeaverequestStatusCancle(int leaveRequestId, int employeeid);

}
public class LeaveService : ILeaveService
{
    private readonly ApiClients.LeaveApi _leaveApi;

    public LeaveService(ApiClients api)
    {
        _leaveApi = api.Leave;
    }

    //Leave Type Methods
    public async Task<ApiResult<List<LeaveTypeViewModel>>> GetLeaveTypeAsync()
        => await _leaveApi.GetAllLeaveType();

    public async Task<ApiResult<LeaveTypeViewModel>> GetLeaveTypeByIdAsync(int id)
        => await _leaveApi.GetLeaveTypeById(id);

    public async Task<ApiResult<LeaveTypeViewModel>> CreateLeaveTypeAsync(LeaveTypeViewModel leaveTypeViewModel)
        => await _leaveApi.CreateLeaveType(leaveTypeViewModel);

    public async Task<ApiResult<bool>> DeleteLeaveTypeAsync(int id)
        => await _leaveApi.DeleteLeaveTypeAsync(id);

    public async Task<ApiResult<bool>> UpdateLeaveTypeAsync(int id, LeaveTypeViewModel viewModel) 
        => await _leaveApi.UpdateLeaveTypeAsync(id, viewModel);

    //Leave Request Methods
    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestAsync()
        => await _leaveApi.GetAllLeaveRequest();

    public async Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByFilterAsync(Status? status, int? empId)
        => await _leaveApi.GetRequestByFilterAsync(status, empId);

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
        => await _leaveApi.CancelLeaveRequest(leaveRequestId, employeeid);
}
