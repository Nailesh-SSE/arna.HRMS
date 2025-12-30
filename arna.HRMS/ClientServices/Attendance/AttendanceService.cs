using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Attendance;

public interface IAttendance
{
    Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
    Task<AttendanceDto?> GetAttendanceByMonthAsync(int year, int month, int empId);
}

public class AttendanceService : IAttendance
{
    private readonly HttpClient _httpClient;

    public AttendanceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
    {
        return await _httpClient
            .GetFromJsonAsync<AttendanceDto>($"api/Attendance/{id}");
    }

    public async Task<AttendanceDto?> GetAttendanceByMonthAsync(int year, int month, int empId)
    {
        return await _httpClient
            .GetFromJsonAsync<AttendanceDto>(
                $"api/Attendance/by-month?year={year}&month={month}&empId={empId}");
    }
}
