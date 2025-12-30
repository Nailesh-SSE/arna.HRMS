using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IClockService
{
    Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto AttendanceDto);
}
public class ClockService(HttpClient HttpClient) : IClockService
{
    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/Attendance", attendanceDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AttendanceDto>();
    }
}
