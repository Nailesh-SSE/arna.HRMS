using System.Net.Http.Json;
using arna.HRMS.Models.DTOs;

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

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId)
    {
        return await _httpClient.GetFromJsonAsync<List<MonthlyAttendanceDto>>(
            $"api/Attendance/by-month?year={year}&month={month}&empId={empId}"
        ) ?? new List<MonthlyAttendanceDto>();
    }
}
