using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Enums;
using arna.HRMS.Models.DTOs;

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
    Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int Id);
    Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, LeaveStatus status, int approvedBy);

    //Leave Balance Methods
    Task<ServiceResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync();
    Task<ServiceResult<bool>> DeleteLeaveBalanceAsync(int id);
    Task<ServiceResult<EmployeeLeaveBalanceDto>> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto leaveBalanceDto);
    Task<ServiceResult<List<EmployeeLeaveBalanceDto?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
}
