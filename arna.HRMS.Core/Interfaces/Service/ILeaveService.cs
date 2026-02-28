using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public interface ILeaveService
{
    // Leave Types
    Task<ServiceResult<List<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<ServiceResult<LeaveTypeDto?>> GetLeaveTypeByIdAsync(int id);
    Task<ServiceResult<LeaveTypeDto>> CreateLeaveTypeAsync(LeaveTypeDto dto);
    Task<ServiceResult<LeaveTypeDto>> UpdateLeaveTypeAsync(LeaveTypeDto dto);
    Task<ServiceResult<bool>> DeleteLeaveTypeAsync(int id);

    // Leave Requests
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsAsync();
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsByFilterAsync(Status? status, int? employeeId);
    Task<ServiceResult<LeaveRequestDto?>> GetLeaveRequestByIdAsync(int id);
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto dto);
    Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto dto);
    Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ServiceResult<bool>> UpdateLeaveRequestStatusAsync(int leaveRequestId, Status status, int approvedBy);
    Task<ServiceResult<bool>> CancelLeaveRequestAsync(int id, int employeeId); 
}