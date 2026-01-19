using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IAttendanceRequestService
{
    Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync();
    Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id);
    Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id);
}
