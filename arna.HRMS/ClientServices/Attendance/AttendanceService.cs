using arna.HRMS.ClientServices.Common;
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
    private readonly HttpService _http;

    public AttendanceService(HttpService http)
    {
        _http = http;
    }
    /*
     public async Task<ApiResult<UserDto>> GetUserByIdAsync(int id)
    {
        return await _http.GetAsync<UserDto>($"api/users/{id}");
    }
     */
    public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
    {
        var result = await _http.GetAsync<AttendanceDto>($"api/Attendance/{id}");
        return result.Data;
    }

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId)
    {
        var apiresult = await _http.GetAsync<List<MonthlyAttendanceDto>>($"api/attendance/by-month?year={year}&month={month}&empId={empId}");
        var apiData = apiresult.Data ?? new List<MonthlyAttendanceDto>();
        return MonthlyAttendanceBuilder.Build(year, month, empId, apiData);
    }
}
