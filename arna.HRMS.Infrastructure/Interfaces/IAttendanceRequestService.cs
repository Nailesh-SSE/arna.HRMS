using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IAttendanceRequestService
{
    Task<List<AttendanceRequestDto>> GetAttendanceRequestAsync();
    Task<AttendanceRequestDto?> GetAttendenceRequestByIdAsync(int id);
    Task<AttendanceRequestDto> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto);
    Task<bool> UpdateAttendanceRequestStatusAsync(int id);
}
