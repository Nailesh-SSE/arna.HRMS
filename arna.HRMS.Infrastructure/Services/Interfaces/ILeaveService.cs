using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface ILeaveService
{
    //Leave Master Methods
    Task<ServiceResult<List<LeaveMasterDto>>> GetLeaveMasterAsync();
    Task<ServiceResult<LeaveMasterDto>> CreateLeaveMasterAsync(LeaveMasterDto leaveMasterDto);
    Task<ServiceResult<LeaveMasterDto>> GetLeaveMasterByIdAsync(int id);
    Task<ServiceResult<bool>> DeleteLeaveMasterAsync(int id);
    Task<ServiceResult<LeaveMasterDto>> UpdateLeaveMasterAsync(LeaveMasterDto leaveMasterDto);
    Task<ServiceResult<bool>> LeaveExistsAsync(string Name);

    //Leave Request Methods
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestAsync();
    Task<ServiceResult<List<LeaveRequestDto>>> GetPendingLeaveRequest();
    Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int Id);
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status, int approvedBy);
    Task<ServiceResult<bool>> UpdateLeaveRequestStatusCancelAsync(int id, int employeeId);

    //Leave Balance Methods
    Task<ServiceResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync();
    Task<ServiceResult<bool>> DeleteLeaveBalanceAsync(int id);
    Task<ServiceResult<EmployeeLeaveBalanceDto>> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto leaveBalanceDto);
    Task<ServiceResult<List<EmployeeLeaveBalanceDto?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
}
