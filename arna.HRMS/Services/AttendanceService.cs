using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IAttendanceService
{
    Task<ApiResult<List<AttendanceViewModel>>> GetAttendanceByEmployeeOrStatusAsync(AttendanceStatus? status, int? employeeId);
    Task<ApiResult<AttendanceViewModel>> GetAttendanceByIdAsync(int id);
    Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date, AttendanceStatus? statusId);
}

public class AttendanceService : IAttendanceService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public AttendanceService(ApiClients api)
    {
        _attendance = api.Attendance;
    }

    public async Task<ApiResult<List<AttendanceViewModel>>> GetAttendanceByEmployeeOrStatusAsync(AttendanceStatus? status, int? employeeId)
    {
        return await _attendance.GetByEmployeeOrStatusAsync(status, employeeId);
    }

    public async Task<ApiResult<AttendanceViewModel>> GetAttendanceByIdAsync(int id)
    {
        return await _attendance.GetByIdAsync(id);
    }

    public async Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date, AttendanceStatus? statusId)
    {
        return await _attendance.GetMonthlyAttendanceAsync(year, month, empId, date, statusId);
    }
}