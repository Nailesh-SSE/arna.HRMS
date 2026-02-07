using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Models.ViewModels.Leave;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Http;

public sealed class ApiClients
{
    public AuthApi Auth { get; }
    public EmployeeApi Employees { get; }
    public DepartmentApi Departments { get; }
    public AttendanceApi Attendance { get; }
    public UserApi Users { get; }
    public AttendanceRequestApi AttendanceRequest { get; }
    public LeaveApi Leave { get; }
    public RoleApi Role { get; }

    public ApiClients(HttpService http)
    {
        http.SetApiClients(this);

        Auth = new(http);
        Employees = new(http);
        Departments = new(http);
        Attendance = new(http);
        Users = new(http);
        AttendanceRequest = new(http);
        Leave = new(http);
        Role = new(http);
    }

    // =====================================================
    // GENERIC CRUD CORE
    // =====================================================

    private sealed class CrudExecutor<T>
    {
        private readonly HttpService _http;
        private readonly string _url;

        public CrudExecutor(HttpService http, string baseUrl)
        {
            _http = http;
            _url = baseUrl;
        }

        public Task<ApiResult<List<T>>> GetAll() =>
            _http.GetAsync<List<T>>(_url);

        public Task<ApiResult<T>> GetById(int id) =>
            _http.GetAsync<T>($"{_url}/{id}");

        public Task<ApiResult<T>> Create(T dto) =>
            _http.PostAsync<T>(_url, dto);

        public Task<ApiResult<T>> Update(int id, T dto) =>
            _http.PostAsync<T>($"{_url}/{id}", dto);

        public Task<ApiResult<bool>> Delete(int id) =>
            _http.DeleteAsync<bool>($"{_url}/{id}");
    }

    private static async Task<ApiResult<bool>> ToBool<T>(
        Task<ApiResult<T>> task,
        string error)
    {
        var result = await task;

        return result.IsSuccess
            ? ApiResult<bool>.Success(true, result.StatusCode)
            : ApiResult<bool>.Fail(result.Message ?? error, result.StatusCode);
    }

    // =====================================================
    // AUTH
    // =====================================================

    public sealed class AuthApi
    {
        private const string Url = "api/auth";
        private readonly HttpService _http;

        public AuthApi(HttpService http) => _http = http;

        public Task<ApiResult<AuthResponse>> Login(LoginRequest req) =>
            _http.PostAsync<AuthResponse>($"{Url}/login", req);

        public Task<ApiResult<AuthResponse>> RefreshToken(RefreshTokenViewModel req) =>
            _http.PostAsync<AuthResponse>($"{Url}/refresh", req);

        public Task<ApiResult<bool>> Logout() =>
            _http.PostAsync<bool>($"{Url}/logout", new { });
    }

    // =====================================================
    // EMPLOYEE
    // =====================================================

    public sealed class EmployeeApi
    {
        private const string Url = "api/employees";
        private readonly CrudExecutor<EmployeeViewModel> _crud;

        public EmployeeApi(HttpService http) =>
            _crud = new(http, Url);

        public Task<ApiResult<List<EmployeeViewModel>>> GetAll() => _crud.GetAll();
        public Task<ApiResult<EmployeeViewModel>> GetById(int id) => _crud.GetById(id);
        public Task<ApiResult<EmployeeViewModel>> Create(EmployeeViewModel dto) => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, EmployeeViewModel dto) =>
            ToBool(_crud.Update(id, dto), "Unable to update employee.");

        public Task<ApiResult<bool>> Delete(int id) => _crud.Delete(id);
    }

    // =====================================================
    // DEPARTMENT
    // =====================================================

    public sealed class DepartmentApi
    {
        private const string Url = "api/department";
        private readonly CrudExecutor<DepartmentViewModel> _crud;

        public DepartmentApi(HttpService http) =>
            _crud = new(http, Url);

        public Task<ApiResult<List<DepartmentViewModel>>> GetAll() => _crud.GetAll();
        public Task<ApiResult<DepartmentViewModel>> GetById(int id) => _crud.GetById(id);
        public Task<ApiResult<DepartmentViewModel>> Create(DepartmentViewModel dto) => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, DepartmentViewModel dto) =>
            ToBool(_crud.Update(id, dto), "Unable to update department.");

        public Task<ApiResult<bool>> Delete(int id) => _crud.Delete(id);
    }

    // =====================================================
    // ATTENDANCE
    // =====================================================

    public sealed class AttendanceApi
    {
        private const string Url = "api/attendance";
        private readonly CrudExecutor<AttendanceViewModel> _crud;
        private readonly HttpService _http;

        public AttendanceApi(HttpService http)
        {
            _http = http;
            _crud = new(http, Url);
        }

        public Task<ApiResult<AttendanceViewModel>> GetById(int id) =>
            _crud.GetById(id);

        public Task<ApiResult<AttendanceViewModel>> Create(AttendanceViewModel dto) =>
            _crud.Create(dto);

        public Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId) =>
            _http.GetAsync<AttendanceViewModel>($"{Url}/clockStatus/{employeeId}");

        public Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetByMonth(
            int year, int month, int? empId, DateTime? date) =>
            _http.GetAsync<List<MonthlyAttendanceViewModel>>(
                $"{Url}/monthly-attendance?year={year}&month={month}&empId={empId}&date={date:yyyy-MM-dd}");
    }

    // =====================================================
    // USER
    // =====================================================

    public sealed class UserApi
    {
        private const string Url = "api/users";
        private readonly CrudExecutor<UserViewModel> _crud;
        private readonly HttpService _http;

        public UserApi(HttpService http)
        {
            _http = http;
            _crud = new(http, Url);
        }

        public Task<ApiResult<List<UserViewModel>>> GetAll() => _crud.GetAll();
        public Task<ApiResult<UserViewModel>> GetById(int id) => _crud.GetById(id);
        public Task<ApiResult<UserViewModel>> Create(UserViewModel dto) => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, UserViewModel dto) =>
            ToBool(_crud.Update(id, dto), "Unable to update user.");

        public Task<ApiResult<bool>> Delete(int id) => _crud.Delete(id);

        public Task<ApiResult<bool>> ChangePassword(int id, string newPassword) =>
            _http.PostAsync<bool>($"{Url}/{id}/change-password", newPassword);
    }

    // =====================================================
    // ATTENDANCE REQUEST
    // =====================================================

    public sealed class AttendanceRequestApi
    {
        private const string Url = "api/attendanceRequest";
        private readonly CrudExecutor<AttendanceRequestViewModel> _crud;
        private readonly HttpService _http;

        public AttendanceRequestApi(HttpService http)
        {
            _http = http;
            _crud = new(http, Url);
        }

        // ================= CORE BUILDER =================

        private Task<ApiResult<List<AttendanceRequestViewModel>>> GetAllInternal(
            int? empId,
            Status? status)
        {
            var query = new List<string>();

            if (empId.HasValue)
                query.Add($"employeeId={empId.Value}");

            if (status.HasValue)
                query.Add($"status={status.Value}");

            var qs = query.Any() ? "?" + string.Join("&", query) : "";

            return _http.GetAsync<List<AttendanceRequestViewModel>>($"{Url}{qs}");
        }

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll() =>
            GetAllInternal(null, null);

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int empId) =>
            GetAllInternal(empId, null);

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(Status status) =>
            GetAllInternal(null, status);

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int empId, Status status) =>
            GetAllInternal(empId, status);

        public Task<ApiResult<AttendanceRequestViewModel>> GetById(int id) =>
            _crud.GetById(id);

        public Task<ApiResult<AttendanceRequestViewModel>> Create(AttendanceRequestViewModel dto) =>
            _crud.Create(dto);

        public Task<ApiResult<bool>> Update(AttendanceRequestViewModel dto) =>
            ToBool(_crud.Update(dto.Id, dto), "Unable to update request.");

        public Task<ApiResult<bool>> Delete(int id) =>
            _crud.Delete(id);

        public Task<ApiResult<bool>> UpdateRequestStatus(int id, Status status) =>
            _http.PostAsync<bool>($"{Url}/status/{id}?status={status}", new { });
    }

    // =====================================================
    // LEAVE
    // =====================================================

    public sealed class LeaveApi
    {
        private const string Url = "api/leave";

        private readonly CrudExecutor<LeaveMasterViewModel> _masters;
        private readonly CrudExecutor<LeaveRequestViewModel> _requests;
        private readonly CrudExecutor<EmployeeLeaveBalanceViewModel> _balances;
        private readonly HttpService _http;

        public LeaveApi(HttpService http)
        {
            _http = http;
            _masters = new(http, $"{Url}/masters");
            _requests = new(http, $"{Url}/requests");
            _balances = new(http, $"{Url}/balances");
        }

        public Task<ApiResult<List<LeaveMasterViewModel>>> GetAllLeaveMaster() => _masters.GetAll();
        public Task<ApiResult<LeaveMasterViewModel>> GetLeaveMasterById(int id) => _masters.GetById(id);
        public Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMaster(LeaveMasterViewModel dto) => _masters.Create(dto);
        public Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel dto) =>
            ToBool(_masters.Update(id, dto), "Unable to update leave master.");
        public Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id) => _masters.Delete(id);

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetAllLeaveRequest() => _requests.GetAll();
        public Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestById(int id) => _requests.GetById(id);
        public Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequest(LeaveRequestViewModel dto) => _requests.Create(dto);
        public Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel dto) =>
            ToBool(_requests.Update(id, dto), "Unable to update leave request.");
        public Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id) => _requests.Delete(id);

        public Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status) =>
            _http.PostAsync<bool>($"{Url}/requests/status/{leaveRequestId}?status={status}", new { });

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetRequestByStatusAsync(Status status) =>
            _http.GetAsync<List<LeaveRequestViewModel>>($"{Url}/requests/{status}");

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByEmployee(int employeeId) =>
            _http.GetAsync<List<LeaveRequestViewModel>>($"{Url}/requests/employee/{employeeId}");

        public Task<ApiResult<bool>> CancelLeaveRequest(int leaveRequestId, int employeeId) =>
            _http.PostAsync<bool>($"{Url}/requests/cancel/{leaveRequestId}?employeeid={employeeId}", new { });

        public Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync() =>
            _balances.GetAll();

        public Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel dto) =>
            ToBool(_balances.Update(id, dto), "Unable to update leave balance.");

        public Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id) =>
            _balances.Delete(id);

        public Task<ApiResult<List<EmployeeLeaveBalanceViewModel?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId) =>
            _http.GetAsync<List<EmployeeLeaveBalanceViewModel?>>($"{Url}/balances/employee/{employeeId}");
    }

    // =====================================================
    // ROLE
    // =====================================================

    public sealed class RoleApi
    {
        private const string Url = "api/role";
        private readonly CrudExecutor<RoleViewModel> _crud;

        public RoleApi(HttpService http) =>
            _crud = new(http, Url);

        public Task<ApiResult<List<RoleViewModel>>> GetAll() => _crud.GetAll();
        public Task<ApiResult<RoleViewModel>> GetById(int id) => _crud.GetById(id);
        public Task<ApiResult<RoleViewModel>> Create(RoleViewModel dto) => _crud.Create(dto);

        public Task<ApiResult<bool>> Update(int id, RoleViewModel dto) =>
            ToBool(_crud.Update(id, dto), "Unable to update role.");

        public Task<ApiResult<bool>> Delete(int id) => _crud.Delete(id);
    }
}
