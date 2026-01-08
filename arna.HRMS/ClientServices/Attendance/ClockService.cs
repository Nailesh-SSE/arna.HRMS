using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IClockService
{
    Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(
        AttendanceDto attendanceDto);
}

public class ClockService : IClockService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public ClockService(ApiClients api)
    {
        _attendance = api.Attendance;
    }

    public Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto)
        => _attendance.Create(attendanceDto);
}
