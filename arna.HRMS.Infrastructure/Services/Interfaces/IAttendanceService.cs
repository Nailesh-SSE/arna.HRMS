using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IAttendanceService
{
    Task<ServiceResult<List<AttendanceDto>>> GetEmployeeAttendanceByStatusAsync(AttendanceStatus? status, int? EmployeeId );
    Task<ServiceResult<AttendanceDto?>> GetAttendenceByIdAsync(int id);
    Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto);
    Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date, AttendanceStatus? statusId);
    Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId);
}
