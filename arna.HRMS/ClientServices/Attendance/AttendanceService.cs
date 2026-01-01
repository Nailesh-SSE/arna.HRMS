using arna.HRMS.Helpers.Attendance;
using arna.HRMS.Models.DTOs;
using System.Net.Http.Json;

namespace arna.HRMS.ClientServices.Attendance;
public interface IAttendanceService
{
    Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
    Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId);
}

public class AttendanceService : IAttendanceService
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

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(
     int year, int month, int empId)
    {
        var apiData = await _httpClient.GetFromJsonAsync<List<MonthlyAttendanceDto>>($"api/attendance/by-month?year={year}&month={month}&empId={empId}") ?? new List<MonthlyAttendanceDto>();

        // 👇 THIS IS WHERE IT HAPPENS
        return MonthlyAttendanceBuilder.Build(year, month, empId, apiData);
    }
}
