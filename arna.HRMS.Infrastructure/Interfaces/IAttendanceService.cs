using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceDto>> GetAttendanceAsync();
    Task<AttendanceDto?> GetAttendenceByIdAsync(int id);
    Task<AttendanceDto> CreateAttendanceAsync(Attendance attendance);
    Task<List<AttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int EmpId); 
}
