using arna.HRMS.Core.Entities;
using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Services.Http;

public sealed class ApiClients
{
    // ===================== Dashboard =======================

    public DashboardApi Dashboard { get; }

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

    // ===================== REPORT =====================
    public ReportApi Report { get; }

    public ApiClients(HttpService http)
    {
        http.SetApiClients(this);

        Dashboard = new DashboardApi(http);

        Auth = new AuthApi(http);
        Users = new UserApi(http);
        Roles = new RoleApi(http);

        Employees = new EmployeeApi(http);
        Departments = new DepartmentApi(http);

        Attendance = new AttendanceApi(http);
        AttendanceRequests = new AttendanceRequestApi(http);

        Leave = new LeaveApi(http);

        FestivalHoliday = new FestivalHolidayApi(http);

        Report = new ReportApi(http);
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
            return await Http.PostAsync<bool>($"{Url}/{id}/delete", new { });
        }
    }

    // =========================================================
    // =================== Dashboard ===========================
    // =========================================================

    public sealed class DashboardApi
    {
        private const string Url = "api/dashboard";
        private readonly HttpService _http;

        public DashboardApi(HttpService http)
        {
            _http = http;
        }

        public async Task<ApiResult<DashboardViewModel>> GetDashboardAsync(
            Status? status,
            int? employeeId)
        {
            var query = new List<string>();

            if (status.HasValue)
                query.Add($"status={status}");

            if (employeeId.HasValue)
                query.Add($"employeeId={employeeId}");

            var queryString = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;

            return await _http.GetAsync<DashboardViewModel>($"{Url}{queryString}");
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

        public async Task<ApiResult<bool>> LogoutAsync(int userId)
        {
            return await _http.PostAsync<bool>($"{Url}/logout", new { userId });
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

        public async Task<ApiResult<AttendanceViewModel>> GetClockStatusAsync(int employeeId)
        {
            return await Http.GetAsync<AttendanceViewModel>($"{Url}/clockStatus/{employeeId}");
        }

        public async Task<ApiResult<List<AttendanceViewModel>>> GetAttendanceByStatusAndEmployeeIdAsync(AttendanceStatus? status, int? employeeId)
        {
            var query = new List<string>();
            if (status.HasValue)
                query.Add($"status={status}");

            if (employeeId.HasValue)
                query.Add($"employeeId={employeeId}");

            var qs = query.Any() ? "?" + string.Join("&", query) : "";

            return await Http.GetAsync<List<AttendanceViewModel>>($"{Url}{qs}");
        }

        public async Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetMonthlyAttendanceAsync(int year, int month, int? empId, DateTime? date, AttendanceStatus? statusId)
        {
            return await Http.GetAsync<List<MonthlyAttendanceViewModel>>($"{Url}/monthly?year={year}&month={month}&employeeId={empId}&date={date:yyyy-MM-dd}&statusId={statusId}");
        }

        public async Task<ApiResult<AttendanceViewModel>> GetTodayLastEntryAsync(int employeeId)
        {
            return await Http.GetAsync<AttendanceViewModel>($"{Url}/{employeeId}/last-today");
        }

        public async Task<ApiResult<List<AttendanceViewModel>>> GetTodayFirstClockInAsync(int employeeId)
        {
            return await Http.GetAsync<List<AttendanceViewModel>>($"{Url}/employees/{employeeId}/today/first-clock-in");
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

        public async Task<ApiResult<bool>> UpdateStatusAsync(int id, Status status, int empId)
        {
            return await Http.PostAsync<bool>(
                $"{Url}/{id}/status?status={status}&approvedBy={empId}",
                new { });
        }

        public async Task<ApiResult<bool>> CancelRequestAsync(int id, int empId)
        {
            return await Http.PostAsync<bool>(
                $"{Url}/{id}/cancel?employeeId={empId}",
                new { });
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

        public async Task<ApiResult<bool>> UpdateStatusAsync(int id, Status status, int empId)
        {
            return await _http.PostAsync<bool>(
                $"{BaseUrl}/requests/{id}/status?status={status}&approvedBy={empId}",
                new { });
        }

        public async Task<ApiResult<List<LeaveRequestViewModel>>> GetByFilterAsync(Status? status, int? empId)
        {
            var query = new List<string>();

            if (status.HasValue)
                query.Add($"status={status}");

            if (empId.HasValue)
                query.Add($"employeeId={empId}");

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

    // ===============================================
    // ===================== Report ==================
    // ===============================================

    public sealed class ReportApi
    {
        private const string Url = "api/report";
        private readonly HttpService _http;

        public ReportApi(HttpService http)
        {
            _http = http;
        }

        public async Task<ApiResult<List<AttendanceReportViewModel>>> GetEmployeesDailyAttendanceReportAsync(
            int? year,
            int? month,
            int? employeeId,
            AttendanceStatus? statusId,
            DeviceType? device,
            DateTime? FromDate,
            DateTime? ToDate)
        {
            var query = new List<string>();

            if (year.HasValue)
                query.Add($"year={year}");

            if (month.HasValue)
                query.Add($"month={month}");

            if (employeeId.HasValue)
                query.Add($"employeeId={employeeId}");

            if (statusId.HasValue)
                query.Add($"statusId={statusId}");

            if (device.HasValue)
                query.Add($"device={device}");

            if (FromDate.HasValue)
                query.Add($"FromDate={FromDate}");

            if (ToDate.HasValue)
                query.Add($"ToDate={ToDate}");

            var qs = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;

            return await _http.GetAsync<List<AttendanceReportViewModel>>(
                $"{Url}/daily-attendance-report{qs}");
        }

        public async Task<ApiResult<List<EmployeeAttendanceReportViewModel>>> GetEmployeesAttendanceReportAsync(
            int? year,
            int? month,
            int? employeeId,
            DateTime? FromDate,
            DateTime? ToDate)
        {
            var query = new List<string>();

            if (year.HasValue)
                query.Add($"year={year}");

            if (month.HasValue)
                query.Add($"month={month}");

            if (employeeId.HasValue)
                query.Add($"employeeId={employeeId}");

            if (FromDate.HasValue)
                query.Add($"FromDate={FromDate}");

            if (ToDate.HasValue)
                query.Add($"ToDate={ToDate}");

            var qs = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;

            return await _http.GetAsync<List<EmployeeAttendanceReportViewModel>>(
                $"{Url}/employees-attendance-report{qs}");
        }
        public async Task<ApiResult<List<LeaveSummaryReportViewModel>>> GetLeaveSummaryReportAsync(
            int? year,
            int? month,
            int? departmentId,
            DateTime? FromDate,
            DateTime? ToDate)
        {
            var query = new List<string>();
            query.Add($"year={year}");
            if (month.HasValue)
                query.Add($"month={month}");
            if (departmentId.HasValue)
                query.Add($"departmentId={departmentId}");
            if (FromDate.HasValue)
                query.Add($"FromDate={FromDate}");
            if (ToDate.HasValue)
                query.Add($"ToDate={ToDate}");
            var qs = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;
            return await _http.GetAsync<List<LeaveSummaryReportViewModel>>(
                $"{Url}/leave-summary-report{qs}");
        }
        public async Task<ApiResult<List<EmployeeLeaveDetailsReportViewModel>>> GetEmployeeLeaveDetailsReportAsync(
            int? year,
            int? month,
            int? employeeId,
            string? EmployeeNumber,
            DateTime? FromDate,
            DateTime? ToDate)
        {
            var query = new List<string>();
            query.Add($"year={year}");
            if (month.HasValue)
                query.Add($"month={month}");
            if (employeeId.HasValue)
                query.Add($"employeeId={employeeId}");
            if (!string.IsNullOrEmpty(EmployeeNumber))
                query.Add($"employeeNumber={EmployeeNumber}");
            if (FromDate.HasValue)
                query.Add($"FromDate={FromDate}");
            if (ToDate.HasValue)
                query.Add($"ToDate={ToDate}");

            var qs = query.Any()
                ? "?" + string.Join("&", query)
                : string.Empty;

            return await _http.GetAsync<List<EmployeeLeaveDetailsReportViewModel>>(
                $"{Url}/employee-leave-details-report{qs}");
        }
    }
}