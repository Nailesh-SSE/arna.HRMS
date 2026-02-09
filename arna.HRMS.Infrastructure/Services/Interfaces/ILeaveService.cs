using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface ILeaveService
{
    //Leave Type Methods
    Task<ServiceResult<List<LeaveTypeDto>>> GetLeaveTypeAsync();
    Task<ServiceResult<LeaveTypeDto>> CreateLeaveTypeAsync(LeaveTypeDto leaveTypeDto);
    Task<ServiceResult<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id);
    Task<ServiceResult<bool>> DeleteLeaveTypeAsync(int id);
    Task<ServiceResult<LeaveTypeDto>> UpdateLeaveTypeAsync(LeaveTypeDto leaveTypeDto);

    //Leave Request Methods
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestAsync();
    Task<ServiceResult<List<LeaveRequestDto>>> GetByFilterAsync(Status? status, int? employeeId);
    Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int Id);
    Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id);
    Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto leaveRequestDto);
    Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status, int approvedBy);
    Task<ServiceResult<bool>> UpdateLeaveRequestStatusCancelAsync(int id, int employeeId);
}
