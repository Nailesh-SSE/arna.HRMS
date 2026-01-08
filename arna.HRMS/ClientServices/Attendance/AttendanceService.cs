using arna.HRMS.ClientServices.Common;
using arna.HRMS.Helpers.Attendance;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IAttendanceService
{
    Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
    Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId);
}

public class AttendanceService : IAttendanceService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public AttendanceService(ApiClients api)
    {
        _attendance = api.Attendance;
    }

    public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
    {
        var result = await _attendance.GetById(id);
        return result.Data;
    }

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId)
    {
        var result = await _attendance.GetByMonth(year, month, empId);

        var apiData = result.Data ?? new List<MonthlyAttendanceDto>();

        return MonthlyAttendanceBuilder.Build(year, month, empId, apiData);
    }
}
