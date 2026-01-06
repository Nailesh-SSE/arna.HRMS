using arna.HRMS.ClientServices.Common;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IClockService
{
    Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto AttendanceDto);
}
public class ClockService : IClockService
{
    private readonly HttpService _http;

    public ClockService(HttpService http)
    {
        _http = http;
    }
 
    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        var result = await _http.PostAsync<AttendanceDto>("api/Attendance", attendanceDto);
        return result.Data;
    }
}
