using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Infrastructure.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceDto>> GetAttendanceAsync();
    Task<AttendanceDto?> GetAttendenceByIdAsync(int id);
    Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto);
    Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int EmpId);
    Task<AttendanceDto?> GetTodayLastEntryAsync(int employeeId);
}
