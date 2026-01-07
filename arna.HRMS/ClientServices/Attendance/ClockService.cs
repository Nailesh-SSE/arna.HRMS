using arna.HRMS.ClientServices.Common;
using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IClockService
{
    Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto AttendanceDto);
    Task<AttendanceDto?> GetLastTodayAsync(int employeeId);
}
public class ClockService : IClockService
{
    private readonly HttpService _http;

    public ClockService(HttpService http)
    {
        _http = http;
    }
 
    public Task<ApiResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto dto)
        => _http.PostAsync<AttendanceDto>("api/Attendance", dto);
    public async Task<AttendanceDto?> GetLastTodayAsync(int employeeId)
    {
        var result =await _http.GetAsync<AttendanceDto>($"api/attendance/lastClockEntry/{employeeId}");
        return result.Data;
    }


}
