using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels.Attendance;

namespace arna.HRMS.ClientServices.Client.Clock;

public interface IClockService
{
    Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model);
    Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId);
}

public class ClockService : IClockService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public ClockService(ApiClients api)
    {
        _attendance = api.Attendance;
    }
 
    public Task<ApiResult<AttendanceViewModel>> CreateAttendanceAsync(AttendanceViewModel model)
        => _attendance.Create(model);

    public async Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId)
        => await _attendance.GetClockStatus(employeeId); 
}
