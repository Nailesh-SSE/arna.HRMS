using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IClockService
{
    Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
    Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto AttendanceDto);
    //Task UpdateAttendanceAsync(int id, UpdateAttendanceRequest dto);
}
public class ClockService(HttpClient HttpClient) : IClockService
{
    public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
    {
        var response = await HttpClient.GetFromJsonAsync<AttendanceDto>($"api/Attendance/{id}");
        return response;
    }

    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/Attendance", attendanceDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AttendanceDto>();
    }
    /*public async Task UpdateAttendanceAsync(int id, UpdateAttendanceRequest dto)
    {
        var response =
            await HttpClient.PutAsJsonAsync($"api/attendance/{id}", dto);

        response.EnsureSuccessStatusCode();
    }*/
}
