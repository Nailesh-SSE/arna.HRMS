using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IAttendanceRequestService
{
    Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestsAsync(int? employeeId, Status? status);
    Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestsAsync();
    Task<ServiceResult<AttendanceRequestDto?>> GetAttendanceRequestByIdAsync(int id);
    Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto dto);
    Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto);
    Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy);
    Task<ServiceResult<bool>> CancelAttendanceRequestAsync(int id, int employeeId);
}