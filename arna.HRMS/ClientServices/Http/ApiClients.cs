using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Models.ViewModels.Leave;
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
    public RoleApi Role { get; }

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
        Role = new RoleApi(http);
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

        public Task<ApiResult<AuthResponse>> RefreshToken(RefreshTokenViewModel request)
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
        private readonly CrudExecutor<EmployeeViewModel> _crud;

        public EmployeeApi(HttpService http)
        {
            _crud = new CrudExecutor<EmployeeViewModel>(http, baseUrl);
        }

        public Task<ApiResult<List<EmployeeViewModel>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<EmployeeViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<EmployeeViewModel>> Create(EmployeeViewModel dto)
            => _crud.Create(dto);

        public async Task<ApiResult<bool>> Update(int id, EmployeeViewModel dto)
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
        private readonly CrudExecutor<DepartmentViewModel> _crud;

        public DepartmentApi(HttpService http)
        {
            _crud = new CrudExecutor<DepartmentViewModel>(http, baseUrl);
        }

        public Task<ApiResult<List<DepartmentViewModel>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<DepartmentViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<DepartmentViewModel>> Create(DepartmentViewModel dto)
            => _crud.Create(dto);

        public async Task<ApiResult<bool>> Update(int id, DepartmentViewModel dto)
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
        private readonly CrudExecutor<AttendanceViewModel> _crud;
        private readonly HttpService _http;

        public AttendanceApi(HttpService http)
        {
            _http = http;
            _crud = new CrudExecutor<AttendanceViewModel>(http, baseUrl);
        }

        public Task<ApiResult<AttendanceViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetByMonth(int year, int month, int empId)
            => _http.GetAsync<List<MonthlyAttendanceViewModel>>($"{baseUrl}/monthly/?year={year}&month={month}&empId={empId}");

        public Task<ApiResult<AttendanceViewModel>> Create(AttendanceViewModel dto)
            => _crud.Create(dto);

        public Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId)
            => _http.GetAsync<AttendanceViewModel>($"{baseUrl}/clockStatus/{employeeId}");
    }

    // =========================
    // USER API (CUSTOM)
    // =========================
    public sealed class UserApi
    {
        private const string baseUrl = "api/users";
        private readonly CrudExecutor<UserViewModel> _crud;
        private readonly HttpService _http;

        public UserApi(HttpService http)
        {
            _http = http;
            _crud = new CrudExecutor<UserViewModel>(http, baseUrl);
        }

        public Task<ApiResult<List<UserViewModel>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<UserViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<UserViewModel>> Create(UserViewModel dto)
            => _crud.Create(dto);

        public async Task<ApiResult<bool>> Update(int id, UserViewModel dto)
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
        private readonly CrudExecutor<AttendanceRequestViewModel> _crud;
        private readonly HttpService _http;

        public AttendanceRequestApi(HttpService http)
        {
            _http = http;
            _crud = new CrudExecutor<AttendanceRequestViewModel>(http, baseUrl);
        }

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<AttendanceRequestViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<AttendanceRequestViewModel>> Create(AttendanceRequestViewModel dto)
            => _crud.Create(dto);

        public Task<ApiResult<AttendanceRequestViewModel>> ApproveRequest(int id)
            => _http.GetAsync<AttendanceRequestViewModel>($"{baseUrl}/status/{id}");
    }

    // ===============================
    // Leave API (CUSTOM)
    // ===============================
    public sealed class LeaveApi
    {
        private const string baseUrl = "api/leave";
        private readonly CrudExecutor<LeaveMasterViewModel> _crudLeaveMaster;
        private readonly CrudExecutor<LeaveRequestViewModel> _crudLeaveRequest;
        private readonly CrudExecutor<EmployeeLeaveBalanceViewModel> _crudLeaveBalance;
        private readonly HttpService _http;

        public LeaveApi(HttpService http)
        {
            _http = http;
            _crudLeaveMaster = new CrudExecutor<LeaveMasterViewModel>(http, $"{baseUrl}/masters");
            _crudLeaveRequest = new CrudExecutor<LeaveRequestViewModel>(http, $"{baseUrl}/requests");
            _crudLeaveBalance = new CrudExecutor<EmployeeLeaveBalanceViewModel>(http, $"{baseUrl}/balances");

        }

        // Leave Master Methods
        public Task<ApiResult<List<LeaveMasterViewModel>>> GetAllLeaveMaster()
            => _crudLeaveMaster.GetAll();

        public Task<ApiResult<LeaveMasterViewModel>> GetLeaveMasterById(int id)
            => _crudLeaveMaster.GetById(id);

        public Task<ApiResult<LeaveMasterViewModel>> CreateLeaveMaster(LeaveMasterViewModel dto)
            => _crudLeaveMaster.Create(dto);

        public async Task<ApiResult<bool>> UpdateLeaveMasterAsync(int id, LeaveMasterViewModel dto)
        {
            var updateResult = await _crudLeaveMaster.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveMasterAsync(int id)
            => _crudLeaveMaster.Delete(id);

        // Leave Request Methods
        public Task<ApiResult<List<LeaveRequestViewModel>>> GetAllLeaveRequest()
            => _crudLeaveRequest.GetAll();

        public Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestById(int id)
            => _crudLeaveRequest.GetById(id);

        public  Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequest(LeaveRequestViewModel dto)
            => _crudLeaveRequest.Create(dto);

        public async Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel dto)
        {
            var updateResult = await _crudLeaveRequest.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id)
            => _crudLeaveRequest.Delete(id);

        public Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status)
        => _http.PostAsync<bool>($"status/{leaveRequestId}?status={status}",null);

        public  Task<ApiResult<List<LeaveRequestViewModel>>> GetPandingLeaveRequestAsync()
            => _http.GetAsync<List<LeaveRequestViewModel>>($"pending");

        // Leave Balance Methods
        public Task<ApiResult<List<EmployeeLeaveBalanceViewModel>>> GetLeaveBalanceAsync()
            => _crudLeaveBalance.GetAll();

        public async Task<ApiResult<bool>> UpdateLeaveBalanceAsync(int id, EmployeeLeaveBalanceViewModel dto)
        {
            var updateResult = await _crudLeaveBalance.UpdateReturnDto(id, dto);
            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update.", updateResult.StatusCode);
            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> DeleteLeaveBalanceAsync(int id)
            => _crudLeaveBalance.Delete(id);

        public Task<ApiResult<List<EmployeeLeaveBalanceViewModel?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
            => _http.GetAsync<List<EmployeeLeaveBalanceViewModel?>>($"{baseUrl}/balance/{employeeId}");
    }

    // =========================
    // ROLE API (CUSTOM)
    // =========================
    public sealed class RoleApi
    {
        private const string baseUrl = "api/role";
        private readonly CrudExecutor<RoleViewModel> _crud; 

        public RoleApi(HttpService http)
        {
            _crud = new CrudExecutor<RoleViewModel>(http, baseUrl);
        }

        public Task<ApiResult<List<RoleViewModel>>> GetAll()
            => _crud.GetAll();

        public Task<ApiResult<RoleViewModel>> GetById(int id)
            => _crud.GetById(id);

        public Task<ApiResult<RoleViewModel>> Create(RoleViewModel dto)
            => _crud.Create(dto);

        public async Task<ApiResult<bool>> Update(int id, RoleViewModel dto)
        {
            var updateResult = await _crud.UpdateReturnDto(id, dto);

            if (!updateResult.IsSuccess)
                return ApiResult<bool>.Fail(updateResult.Message ?? "Unable to update user.", updateResult.StatusCode);

            return ApiResult<bool>.Success(true, updateResult.StatusCode);
        }

        public Task<ApiResult<bool>> Delete(int id)
            => _crud.Delete(id);
    }
}
