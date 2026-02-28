using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IAttendanceService
{
    Task<ServiceResult<List<AttendanceDto>>> GetEmployeeAttendanceByStatusAsync(AttendanceStatus? status, int? employeeId);
    Task<ServiceResult<AttendanceDto?>> GetAttendanceByIdAsync(int id);
    Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto);
    Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? employeeId, DateTime? date, AttendanceStatus? status);
    Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId);
}