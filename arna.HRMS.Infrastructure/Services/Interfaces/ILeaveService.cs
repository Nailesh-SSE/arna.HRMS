using arna.HRMS.Core.Enums;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface ILeaveService
{
    //Leave Master Methods
    Task<List<LeaveMasterDto>> GetLeaveMasterAsync();
    Task<LeaveMasterDto> CreateLeaveMasterAsync(LeaveMasterDto leaveMasterDto);
    Task<LeaveMasterDto> GetLeaveMasterByIdAsync(int id);
    Task<bool> DeleteLeaveMasterAsync(int id);
    Task<LeaveMasterDto> UpdateLeaveMasterAsync(LeaveMasterDto leaveMasterDto);
    Task<bool> LeaveExistsAsync(string Name);

    //Leave Request Methods
    Task<List<LeaveRequestDto>> GetLeaveRequestAsync();
    Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int Id);
    Task<LeaveRequestDto> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<bool> DeleteLeaveRequestAsync(int id);
    Task<LeaveRequestDto> UpdateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<bool> UpdateStatusLeaveAsync(int leaveRequestId, LeaveStatus status, int approvedBy);

    //Leave Balance Methods
    Task<List<EmployeeLeaveBalanceDto>> GetLeaveBalanceAsync();
    Task<bool> DeleteLeaveBalanceAsync(int id);
    Task<EmployeeLeaveBalanceDto> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto leaveBalanceDto);
    Task<EmployeeLeaveBalanceDto?> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
}
