using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Client.Clock;

public interface IClockService
{
    Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto AttendanceDto);
    Task<ApiResult<AttendanceDto>> GetClockStatus(int employeeId);
}

public class ClockService : IClockService
{
    private readonly ApiClients.AttendanceApi _attendance;

    public ClockService(ApiClients api)
    {
        _attendance = api.Attendance;
    }
 
    public Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto dto)
        => _attendance.Create(dto);

    public async Task<ApiResult<AttendanceDto>> GetClockStatus(int employeeId)
        => await _attendance.GetClockStatus(employeeId); 
}
