using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Services.Http;

public sealed class ApiClients
{
    // ===================== AUTH & ACCESS =====================

    public AuthApi Auth { get; }
    public UserApi Users { get; }
    public RoleApi Roles { get; }

    // ===================== CORE HR =====================

    public EmployeeApi Employees { get; }
    public DepartmentApi Departments { get; }

    // ===================== ATTENDANCE =====================

    public AttendanceApi Attendance { get; }
    public AttendanceRequestApi AttendanceRequests { get; }

    // ===================== LEAVE =====================

    public LeaveApi Leave { get; }

    public FestivalHolidayApi FestivalHoliday { get; }

    public ApiClients(HttpService http)
    {
        http.SetApiClients(this);

        Auth = new AuthApi(http);
        Users = new UserApi(http);
        Roles = new RoleApi(http);

        Employees = new EmployeeApi(http);
        Departments = new DepartmentApi(http);

        Attendance = new AttendanceApi(http);
        AttendanceRequests = new AttendanceRequestApi(http);

        Leave = new LeaveApi(http);

        FestivalHoliday = new FestivalHolidayApi(http);
    }

    // =========================================================
    // ===================== BASE CRUD =========================
    // =========================================================

    public abstract class BaseCrudApi<T>
    {
        protected readonly HttpService Http;
        protected readonly string Url;

        protected BaseCrudApi(HttpService http, string url)
        {
            Http = http;
            Url = url;
        }

        public async Task<ApiResult<List<T>>> GetAllAsync()
        {
            return await Http.GetAsync<List<T>>(Url);
        }

        public async Task<ApiResult<T>> GetByIdAsync(int id)
        {
            return await Http.GetAsync<T>($"{Url}/{id}");
        }

        public async Task<ApiResult<T>> CreateAsync(T dto)
        {
            return await Http.PostAsync<T>(Url, dto);
        }

        // UPDATE via POST (your backend pattern)
        public async Task<ApiResult<bool>> UpdateAsync(int id, T dto)
        {
            var result = await Http.PostAsync<T>($"{Url}/{id}", dto);

            return result.IsSuccess
                ? ApiResult<bool>.Success(true, result.StatusCode)
                : ApiResult<bool>.Fail(result.Message ?? "Update failed", result.StatusCode);
        }

        public async Task<ApiResult<bool>> DeleteAsync(int id)
        {
            return await Http.DeleteAsync<bool>($"{Url}/{id}");
        }
    }

    // =========================================================
    // ===================== AUTH ==============================
    // =========================================================

    public sealed class AuthApi
    {
        private const string Url = "api/auth";
        private readonly HttpService _http;

        public AuthApi(HttpService http)
        {
            _http = http;
        }

        public async Task<ApiResult<AuthResponse>> LoginAsync(LoginViewModel request)
        {
            return await _http.PostAsync<AuthResponse>($"{Url}/login", request);
        }

        public async Task<ApiResult<AuthResponse>> RefreshTokenAsync(RefreshToken request)
        {
            return await _http.PostAsync<AuthResponse>($"{Url}/refresh", request);
        }

        public async Task<ApiResult<bool>> LogoutAsync()
        {
            return await _http.PostAsync<bool>($"{Url}/logout", new { });
        }
    }

    // =========================================================
    // ===================== USER ==============================
    // =========================================================

    public sealed class UserApi : BaseCrudApi<UserViewModel>
    {
        public UserApi(HttpService http) : base(http, "api/users") { }

        public async Task<ApiResult<bool>> ChangePasswordAsync(int id, string newPassword)
        {
            return await Http.PostAsync<bool>($"{Url}/{id}/change-password", newPassword);
        }
    }

    // =========================================================
    // ===================== ROLE ==============================
    // =========================================================

    public sealed class RoleApi : BaseCrudApi<RoleViewModel>
    {
        public RoleApi(HttpService http) : base(http, "api/role") { } 
    }

    // =========================================================
    // ===================== EMPLOYEE ==========================
    // =========================================================

    public sealed class EmployeeApi : BaseCrudApi<EmployeeViewModel>
    {
        public EmployeeApi(HttpService http) : base(http, "api/employees") { }
    }

    // =========================================================
    // ===================== DEPARTMENT ========================
    // =========================================================

    public sealed class DepartmentApi : BaseCrudApi<DepartmentViewModel>
    {
        public DepartmentApi(HttpService http) : base(http, "api/department") { }
    }

    // =========================================================
    // ===================== ATTENDANCE ========================
    // =========================================================

    public sealed class AttendanceApi : BaseCrudApi<AttendanceViewModel>
    {
        public AttendanceApi(HttpService http) : base(http, "api/attendance") { }

        public async Task<ApiResult<List<AttendanceViewModel>>> GetByEmployeeOrStatusAsync(AttendanceStatus? status, int? employeeId)
        {
            return await Http.GetAsync<List<AttendanceViewModel>>($"{Url}/employee-attendance?status={status}&employeeId={employeeId}");
        }

        public async Task<ApiResult<AttendanceViewModel>> GetClockStatusAsync(int employeeId)
        {
            return await Http.GetAsync<AttendanceViewModel>($"{Url}/clockStatus/{employeeId}");
        }

        public async Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetMonthlyAttendanceAsync(int year, int month, int? empId, DateTime? date, AttendanceStatus? statusId)
        {
            return await Http.GetAsync<List<MonthlyAttendanceViewModel>>($"{Url}/monthly-attendance?year={year}&month={month}&empId={empId}&date={date:yyyy-MM-dd}&statusId={statusId}");
        }
    }

    // =========================================================
    // ================== ATTENDANCE REQUEST ===================
    // =========================================================

    public sealed class AttendanceRequestApi : BaseCrudApi<AttendanceRequestViewModel>
    {
        public AttendanceRequestApi(HttpService http) : base(http, "api/attendancerequest") { }

        private async Task<ApiResult<List<AttendanceRequestViewModel>>> Filter(int? empId, Status? status)
        {
            var query = new List<string>();

            if (empId.HasValue)
                query.Add($"employeeId={empId}");

            if (status.HasValue)
                query.Add($"status={status}");

            var qs = query.Any() ? "?" + string.Join("&", query) : "";

            return await Http.GetAsync<List<AttendanceRequestViewModel>>($"{Url}{qs}");
        }

        public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll() 
        { 
            return await Filter(null, null); 
        }

        public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int? empId)
        {
            return await Filter(empId, null);
        }

        public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(Status? status)
        { 
            return await Filter(null, status); 
        }

        public async Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int? empId, Status? status)
        {
            return await Filter(empId, status);
        }

        public async Task<ApiResult<bool>> UpdateStatusAsync(int id, Status status)
        {
            return await Http.PostAsync<bool>($"{Url}/{id}/status?status={status}", new { });
        }

        public async Task<ApiResult<bool>> CancelRequestAsync(int id)
        {
            return await Http.PostAsync<bool>($"{Url}/{id}/cancel", new { }); 
        }
    }

    // =========================================================
    // ===================== LEAVE =============================
    // =========================================================

    public sealed class LeaveApi
    {
        private const string BaseUrl = "api/leave";
        private readonly HttpService _http;

        public BaseCrudApi<LeaveTypeViewModel> LeaveTypes { get; }
        public BaseCrudApi<LeaveRequestViewModel> LeaveRequests { get; }

        public LeaveApi(HttpService http)
        {
            _http = http;

            LeaveTypes = new CrudApi<LeaveTypeViewModel>(http, $"{BaseUrl}/types");
            LeaveRequests = new CrudApi<LeaveRequestViewModel>(http, $"{BaseUrl}/requests");
        }

        private sealed class CrudApi<T> : BaseCrudApi<T>
        {
            public CrudApi(HttpService http, string url) : base(http, url) { }
        }

        public async Task<ApiResult<bool>> UpdateStatusAsync(int id, Status status)
        {
            return await _http.PostAsync<bool>($"{BaseUrl}/requests/{id}/status?status={status}", new { });
        }

        public async Task<ApiResult<List<LeaveRequestViewModel>>> GetByFilterAsync(Status? status, int? empId)
        {
            var query = new List<string>();

            if (status.HasValue)
                query.Add($"status={status}");

            if (empId.HasValue)
                query.Add($"empId={empId}");

            var queryString = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;

            return await _http.GetAsync<List<LeaveRequestViewModel>>($"{BaseUrl}/requests/filter{queryString}");
        }

        public async Task<ApiResult<bool>> CancelAsync(int leaveRequestId, int employeeId)
        {
            return await _http.PostAsync<bool>($"{BaseUrl}/requests/{leaveRequestId}/cancel?employeeId={employeeId}", new { });
        }
    }

    // =========================================================
    // ===================== FESTIVAL HOLIDAY ==================
    // =========================================================

    public sealed class FestivalHolidayApi : BaseCrudApi<FestivalHolidayViewModel>
    {
        public FestivalHolidayApi(HttpService http) : base(http, "api/festivalholiday") { }

        public async Task<ApiResult<List<FestivalHolidayViewModel>>> GetByNameAsync(string name)
        {
            return await Http.GetAsync<List<FestivalHolidayViewModel>>($"{Url}/by-name?name={name}");
        }

        public async Task<ApiResult<List<FestivalHolidayViewModel>>> GetByMonthAsync(int year, int month)
        {
            return await Http.GetAsync<List<FestivalHolidayViewModel>>($"{Url}/monthly?year={year}&month={month}");
        }
    }
}