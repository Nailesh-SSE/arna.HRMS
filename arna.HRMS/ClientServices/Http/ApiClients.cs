using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;
using arna.HRMS.Models.Enums;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Http;

public class ApiClients
{
    public AuthApi Auth { get; }
    public EmployeeApi Employees { get; }
    public DepartmentApi Departments { get; }
    public AttendanceApi Attendance { get; }
    public UserApi Users { get; }
    public AttendanceRequestApi AttendanceRequest { get; }
    public LeaveApi Leave { get; }

    public ApiClients(HttpService http)
    {
        http.SetApiClients(this);
        Auth = new AuthApi(http);
        Employees = new EmployeeApi(http);
        Departments = new DepartmentApi(http);
        Attendance = new AttendanceApi(http);
        Users = new UserApi(http);
        AttendanceRequest = new AttendanceRequestApi(http);
        Leave = new LeaveApi(http);
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

        public Task<ApiResult<T>> UpdateReturnDto(int id, T dto)
            => _http.PostAsync<T>($"{_baseUrl}/{id}", dto);

        public Task<ApiResult<bool>> Delete(int id)
            => _http.DeleteAsync<bool>($"{_baseUrl}/{id}");
    }

    // =========================
    // AUTH API
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

        public Task<ApiResult<AuthResponse>> RefreshToken(RefreshTokenRequestDto request)
            => _http.PostAsync<AuthResponse>($"{baseUrl}/refresh", request);

        public Task<ApiResult<bool>> Logout()
            => _http.PostAsync<bool>($"{baseUrl}/logout", new { });
    }

    // =========================
    // EMPLOYEE API
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

        public async Task<ApiResult<bool>> Update(int id, EmployeeDto dto)
        {
            var updateResult = await _crud.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update employee.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

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

        public async Task<ApiResult<bool>> Update(int id, DepartmentDto dto)
        {
            var updateResult = await _crud.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update department.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

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

        public async Task<ApiResult<bool>> Update(int id, UserDto dto)
        {
            var updateResult = await _crud.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update user.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> Delete(int id)
            => _crud.Delete(id);

        public Task<ApiResult<bool>> ChangePassword(int id, string newPassword)
            => _http.PostAsync<bool>($"{baseUrl}/{id}/changepassword", newPassword);
    }

    // ===============================
    // ATTENDANCEREQUEST API (CUSTOM)
    // ===============================
    public sealed class AttendanceRequestApi
    {
        private const string baseUrl = "api/attendanceRequest";
        private readonly CrudExecutor<AttendanceRequestDto> _crud;
        private readonly HttpService _http;

        public AttendanceRequestApi(HttpService http)
        {
            _http = http;
            _crud = new CrudExecutor<AttendanceRequestDto>(http, baseUrl);
        }

        public Task<ApiResult<List<AttendanceRequestDto>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<AttendanceRequestDto>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<AttendanceRequestDto>> Create(AttendanceRequestDto dto)
            => _crud.Create(dto);

        public Task<ApiResult<AttendanceRequestDto>> ApproveRequest(int id)
            => _http.GetAsync<AttendanceRequestDto>($"{baseUrl}/approveRequest/{id}");
    }

    // ===============================
    // Leave API (CUSTOM)
    // ===============================
    public sealed class LeaveApi
    {
        private const string baseUrl = "api/leave";
        private readonly CrudExecutor<LeaveMasterDto> _crudLeaveMaster;
        private readonly CrudExecutor<LeaveRequestDto> _crudLeaveRequest;
        private readonly CrudExecutor<EmployeeLeaveBalanceDto> _crudLeaveBalance;
        private readonly HttpService _http;

        public LeaveApi(HttpService http)
        {
            _http = http;
            _crudLeaveMaster = new CrudExecutor<LeaveMasterDto>(http, $"{baseUrl}/masters");
            _crudLeaveRequest = new CrudExecutor<LeaveRequestDto>(http, $"{baseUrl}/requests");
            _crudLeaveBalance = new CrudExecutor<EmployeeLeaveBalanceDto>(http, $"{baseUrl}/balances");

        }

        // Leave Master Methods
        public Task<ApiResult<List<LeaveMasterDto>>> GetAllLeaveMaster()
            => _crudLeaveMaster.GetAll();

        public Task<ApiResult<LeaveMasterDto>> GetLeaveMasterById(int id)
            => _crudLeaveMaster.GetById(id);

        public Task<ApiResult<LeaveMasterDto>> CreateLeaveMaster(LeaveMasterDto dto)
            => _crudLeaveMaster.Create(dto);

        public async Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterDto dto)
        {
            var updateResult = await _crudLeaveMaster.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id)
            => _crudLeaveMaster.Delete(id);

        // Leave Request Methods
        public Task<ApiResult<List<LeaveRequestDto>>> GetAllLeaveRequest()
            => _crudLeaveRequest.GetAll();

        public Task<ApiResult<LeaveRequestDto>> GetLeaveRequestById(int id)
            => _crudLeaveRequest.GetById(id);

        public  Task<ApiResult<LeaveRequestDto>> CreateLeaveRequest(LeaveRequestDto dto)
            => _crudLeaveRequest.Create(dto);

        public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id,LeaveRequestDto dto)
        {
            var updateResult = await _crudLeaveRequest.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
            => _crudLeaveRequest.Delete(id);

        public Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, CommonStatus status)
        => _http.PostAsync<bool>($"status/{leaveRequestId}?status={status}",null);

        public  Task<ApiResult<List<LeaveRequestDto>>> GetPandingLeaveRequestAsync()
            => _http.GetAsync<List<LeaveRequestDto>>($"pending");

        // Leave Balance Methods
        public Task<ApiResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync()
            => _crudLeaveBalance.GetAll();

        public async Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceDto dto)
        {
            var updateResult = await _crudLeaveBalance.UpdateReturnDto(id, dto);
            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);
            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id)
            => _crudLeaveBalance.Delete(id);

        public Task<ApiResult<List<EmployeeLeaveBalanceDto?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
            => _http.GetAsync<List<EmployeeLeaveBalanceDto?>>($"{baseUrl}/balance/{employeeId}");
    }
}
