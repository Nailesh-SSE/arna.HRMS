using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.ClientServices.Client.Attendance;

public interface IAttendanceService
{
    Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id);
    Task<List<MonthlyAttendanceViewModel>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date); 
}

public class AttendanceService : IAttendanceService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public AttendanceService(ApiClients api)
    {
        _attendance = api.Attendance;
    }

    public async Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id)
    {
        var result = await _attendance.GetById(id);
        return result.Data;
    }

    public async Task<List<MonthlyAttendanceViewModel>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date)
    {
        var result = await _attendance.GetByMonth(year, month, empId, date);
        return result.Data ?? new List<MonthlyAttendanceViewModel>();
    }
}
