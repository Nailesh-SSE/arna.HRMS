using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Enums;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IAttendanceRequestService
{
    Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync();
    Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id);
    Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, CommonStatusList status, int approvedBy);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusCancleAsync(int id, int EmployeeId);
}
