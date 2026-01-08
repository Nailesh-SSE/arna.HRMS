using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Common;

public class ApiClients
{
    public AuthApi Auth { get; }
    public EmployeeApi Employees { get; }
    public DepartmentApi Departments { get; }
    public AttendanceApi Attendance { get; }
    public UserApi Users { get; }

    public ApiClients(HttpService http)
    {
        Auth = new AuthApi(http);
        Employees = new EmployeeApi(http);
        Departments = new DepartmentApi(http);
        Attendance = new AttendanceApi(http);
        Users = new UserApi(http);
    }

    // =========================
    // GENERIC CRUD EXECUTOR
    // =========================
    private sealed class CrudExecutor<T>
    {
        private readonly HttpService _http;
        private readonly string _baseUrl;

        public CrudExecutor(HttpService http, string baseUrl)
        {
            _http = http;
            _baseUrl = baseUrl;
        }

        public Task<ApiResult<List<T>>> GetAll()
            => _http.GetAsync<List<T>>(_baseUrl); 

        public Task<ApiResult<T>> GetById(int id)
            => _http.GetAsync<T>($"{_baseUrl}/{id}");

        public Task<ApiResult<T>> Create(T dto)
            => _http.PostAsync<T>(_baseUrl, dto);

        public Task<ApiResult<bool>> Update(int id, T dto)
            => _http.PutAsync<bool>($"{_baseUrl}/{id}", dto);

        public Task<ApiResult<bool>> Delete(int id)
            => _http.DeleteAsync<bool>($"{_baseUrl}/{id}");
    }

    // =========================
    // AUTH API (ALL URLs)
    // =========================
    public sealed class AuthApi
    {
        private const string baseUrl = "api/auth";
        private readonly HttpService _http;

        public AuthApi(HttpService http)
        {
            _http = http;
        }

        public Task<ApiResult<AuthResponse>> Login(LoginRequest request)
            => _http.PostAsync<AuthResponse>($"{baseUrl}/login", request);

        public Task<ApiResult<bool>> Logout()
            => _http.PostAsync<bool>($"{baseUrl}/logout", new { });
    }

    // =========================
    // EMPLOYEE API (ALL URLs)
    // =========================
    public sealed class EmployeeApi
    {
        private const string baseUrl = "api/employees";
        private readonly CrudExecutor<EmployeeDto> _crud;

        public EmployeeApi(HttpService http)
        {
            _crud = new CrudExecutor<EmployeeDto>(http, baseUrl);
        }

        public Task<ApiResult<List<EmployeeDto>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<EmployeeDto>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<EmployeeDto>> Create(EmployeeDto dto)
            => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, EmployeeDto dto)
            => _crud.Update(id, dto);

        public Task<ApiResult<bool>> Delete(int id)
            => _crud.Delete(id);
    }

    // =========================
    // DEPARTMENT API
    // =========================
    public sealed class DepartmentApi
    {
        private const string baseUrl = "api/department";
        private readonly CrudExecutor<DepartmentDto> _crud;

        public DepartmentApi(HttpService http)
        {
            _crud = new CrudExecutor<DepartmentDto>(http, baseUrl);
        }

        public Task<ApiResult<List<DepartmentDto>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<DepartmentDto>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<DepartmentDto>> Create(DepartmentDto dto)
            => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, DepartmentDto dto)
            => _crud.Update(id, dto);

        public Task<ApiResult<bool>> Delete(int id)
            => _crud.Delete(id);
    }

    // =========================
    // ATTENDANCE API (CUSTOM)
    // =========================
    public sealed class AttendanceApi
    {
        private const string baseUrl = "api/attendance";
        private readonly CrudExecutor<AttendanceDto> _crud;
        private readonly HttpService _http;

        public AttendanceApi(HttpService http)
        {
            _http = http;
            _crud = new CrudExecutor<AttendanceDto>(http, baseUrl);
        }

        public Task<ApiResult<AttendanceDto>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<List<MonthlyAttendanceDto>>> GetByMonth(int year, int month, int empId)
            => _http.GetAsync<List<MonthlyAttendanceDto>>($"{baseUrl}/monthly/?year={year}&month={month}&empId={empId}");

        public Task<ApiResult<AttendanceDto>> Create(AttendanceDto dto)
            => _crud.Create(dto);

        public Task<ApiResult<AttendanceDto>> GetClockStatus(int employeeId)
            => _http.GetAsync<AttendanceDto>($"{baseUrl}/clockStatus/{employeeId}"); 

    }

    // =========================
    // USER API (CUSTOM)
    // =========================
    public sealed class UserApi
    {
        private const string baseUrl = "api/users";
        private readonly CrudExecutor<UserDto> _crud;
        private readonly HttpService _http;

        public UserApi(HttpService http)
        {
            _http = http; 
            _crud = new CrudExecutor<UserDto>(http, baseUrl);
        }

        public Task<ApiResult<List<UserDto>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<UserDto>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<UserDto>> Create(UserDto dto)
            => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, UserDto dto)
            => _crud.Update(id, dto);

        public Task<ApiResult<bool>> Delete(int id)
            => _crud.Delete(id);

        public Task<ApiResult<bool>> ChangePassword(int id, string newPassword)
            => _http.PutAsync<bool>($"{baseUrl}/{id}/changepassword", newPassword);
    }
}
