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

        Auth = new(http);
        Users = new(http);
        Roles = new(http);

        Employees = new(http);
        Departments = new(http);

        Attendance = new(http);
        AttendanceRequests = new(http);

        Leave = new(http);
        FestivalHoliday = new(http);
    }

    // ===================== SHARED CRUD CORE =====================

    public abstract class BaseCrudApi<T>
    {
        protected readonly HttpService Http;
        protected readonly string Url;

        protected BaseCrudApi(HttpService http, string url)
        {
            Http = http;
            Url = url;
        }

        public Task<ApiResult<List<T>>> GetAll() =>
            Http.GetAsync<List<T>>(Url);

        public Task<ApiResult<T>> GetById(int id) =>
            Http.GetAsync<T>($"{Url}/{id}");

        public Task<ApiResult<T>> Create(T dto) =>
            Http.PostAsync<T>(Url, dto);

        public Task<ApiResult<bool>> Delete(int id) =>
            Http.DeleteAsync<bool>($"{Url}/{id}");

        protected Task<ApiResult<T>> UpdateRaw(int id, T dto) =>
            Http.PostAsync<T>($"{Url}/{id}", dto);
    }

    protected static async Task<ApiResult<bool>> ToBool<T>(
        Task<ApiResult<T>> task,
        string error)
    {
        var result = await task;

        return result.IsSuccess
            ? ApiResult<bool>.Success(true, result.StatusCode)
            : ApiResult<bool>.Fail(result.Message ?? error, result.StatusCode);
    }

    // ===================== AUTH =====================

    public sealed class AuthApi
    {
        private const string Url = "api/auth";
        private readonly HttpService _http;

        public AuthApi(HttpService http) => _http = http;

        public async Task<ApiResult<AuthResponse>> Login(LoginRequest req)
        {
            try
            {
                return await _http.PostAsync<AuthResponse>($"{Url}/login", req);
            }
            catch (Exception)
            {

                throw;
            }
            
        }


        public Task<ApiResult<AuthResponse>> RefreshToken(RefreshTokenViewModel req) =>
            _http.PostAsync<AuthResponse>($"{Url}/refresh", req);

        public Task<ApiResult<bool>> Logout() =>
            _http.PostAsync<bool>($"{Url}/logout", new { });
    }

    // ===================== USER =====================

    public sealed class UserApi : BaseCrudApi<UserViewModel>
    {
        public UserApi(HttpService http) : base(http, "api/users") { }

        public Task<ApiResult<bool>> Update(int id, UserViewModel dto) =>
            ToBool(UpdateRaw(id, dto), "Unable to update user.");

        public Task<ApiResult<bool>> ChangePassword(int id, string newPassword) =>
            Http.PostAsync<bool>($"{Url}/{id}/change-password", newPassword);
    }

    // ===================== ROLE =====================

    public sealed class RoleApi : BaseCrudApi<RoleViewModel>
    {
        public RoleApi(HttpService http) : base(http, "api/role") { }

        public Task<ApiResult<bool>> Update(int id, RoleViewModel dto) =>
            ToBool(UpdateRaw(id, dto), "Unable to update role.");
    }

    // ===================== EMPLOYEE =====================

    public sealed class EmployeeApi : BaseCrudApi<EmployeeViewModel>
    {
        public EmployeeApi(HttpService http) : base(http, "api/employees") { }

        public Task<ApiResult<bool>> Update(int id, EmployeeViewModel dto) =>
            ToBool(UpdateRaw(id, dto), "Unable to update employee.");
    }

    // ===================== DEPARTMENT =====================

    public sealed class DepartmentApi : BaseCrudApi<DepartmentViewModel>
    {
        public DepartmentApi(HttpService http) : base(http, "api/department") { }

        public Task<ApiResult<bool>> Update(int id, DepartmentViewModel dto) =>
            ToBool(UpdateRaw(id, dto), "Unable to update department.");
    }

    // ===================== ATTENDANCE =====================

    public sealed class AttendanceApi : BaseCrudApi<AttendanceViewModel>
    {
        public AttendanceApi(HttpService http) : base(http, "api/attendance") { }

        public Task<ApiResult<List<AttendanceViewModel>>> GetByEmployeeOrStatus(AttendanceStatuses? status, int? employeeId) =>
            Http.GetAsync<List<AttendanceViewModel>>(
                $"{Url}/employee-attendance?status={status}&employeeId={employeeId}");

        public Task<ApiResult<AttendanceViewModel>> GetClockStatus(int employeeId) =>
            Http.GetAsync<AttendanceViewModel>($"{Url}/clockStatus/{employeeId}");

        public Task<ApiResult<List<MonthlyAttendanceViewModel>>> GetByMonth(
            int year, int month, int? empId, DateTime? date, AttendanceStatuses? statusId) =>
            Http.GetAsync<List<MonthlyAttendanceViewModel>>(
                $"{Url}/monthly-attendance?year={year}&month={month}&empId={empId}&date={date:yyyy-MM-dd}&statusId={statusId}");
    }

    // ===================== ATTENDANCE REQUEST =====================

    public sealed class AttendanceRequestApi : BaseCrudApi<AttendanceRequestViewModel>
    {
        public AttendanceRequestApi(HttpService http)
            : base(http, "api/attendanceRequest") { }

        private Task<ApiResult<List<AttendanceRequestViewModel>>> Filter(
            int? empId, Status? status)
        {
            var query = new List<string>();

            if (empId.HasValue) query.Add($"employeeId={empId}");
            if (status.HasValue) query.Add($"status={status}");

            var qs = query.Any() ? "?" + string.Join("&", query) : "";

            return Http.GetAsync<List<AttendanceRequestViewModel>>($"{Url}{qs}");
        }

        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll() => Filter(null, null);
        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int? empId) => Filter(empId, null);
        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(Status? status) => Filter(null, status);
        public Task<ApiResult<List<AttendanceRequestViewModel>>> GetAll(int? empId, Status? status) => Filter(empId, status);

        public Task<ApiResult<bool>> Update(AttendanceRequestViewModel dto) =>
            ToBool(UpdateRaw(dto.Id, dto), "Unable to update request.");

        public Task<ApiResult<bool>> UpdateRequestStatus(int id, Status status) =>
            Http.PostAsync<bool>($"{Url}/status/{id}?status={status}", new { });
    }

    // ===================== LEAVE =====================

    public sealed class LeaveApi
    {
        private const string Url = "api/leave";
        private readonly HttpService _http;

        private readonly CrudApi<LeaveTypeViewModel> _types;
        private readonly CrudApi<LeaveRequestViewModel> _requests;

        // ---------- Local wrapper over your BaseCrudApi ----------

        private sealed class CrudApi<T> : BaseCrudApi<T>
        {
            public CrudApi(HttpService http, string url) : base(http, url) { }

            public Task<ApiResult<bool>> Update(int id, T dto, string error) =>
                ToBool(UpdateRaw(id, dto), error);
        }

        // ---------- Constructor ----------

        public LeaveApi(HttpService http)
        {
            _http = http;

            _types = new(http, $"{Url}/Types");
            _requests = new(http, $"{Url}/requests");
        }

        // ===================== LEAVE TYPES =====================

        public Task<ApiResult<List<LeaveTypeViewModel>>> GetAllLeaveType() =>
            _types.GetAll();

        public Task<ApiResult<LeaveTypeViewModel>> GetLeaveTypeById(int id) =>
            _types.GetById(id);

        public Task<ApiResult<LeaveTypeViewModel>> CreateLeaveType(LeaveTypeViewModel dto) =>
            _types.Create(dto);

        public Task<ApiResult<bool>> UpdateLeaveTypeAsync(int id, LeaveTypeViewModel dto) =>
            _types.Update(id, dto, "Unable to update leave Type.");

        public Task<ApiResult<bool>> DeleteLeaveTypeAsync(int id) =>
            _types.Delete(id);

        // ===================== LEAVE REQUESTS =====================

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetAllLeaveRequest() =>
            _requests.GetAll();

        public Task<ApiResult<LeaveRequestViewModel>> GetLeaveRequestById(int id) =>
            _requests.GetById(id);

        public Task<ApiResult<LeaveRequestViewModel>> CreateLeaveRequest(LeaveRequestViewModel dto) =>
            _requests.Create(dto);

        public Task<ApiResult<bool>> UpdateLeaveRequestAsync(int id, LeaveRequestViewModel dto) =>
            _requests.Update(id, dto, "Unable to update leave request.");

        public Task<ApiResult<bool>> DeleteLeaveRequestAsync(int id) =>
            _requests.Delete(id);

        // ===================== EXTRA ROUTES =====================

        public Task<ApiResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status)
            => _http.PostAsync<bool>($"{Url}/requests/status/{leaveRequestId}?status={status}", new { });

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetRequestByFilterAsync(Status? status, int? empId)
            => _http.GetAsync<List<LeaveRequestViewModel>>($"{Url}/requests/filter?status={status}&empId={empId}");

        public Task<ApiResult<List<LeaveRequestViewModel>>> GetLeaveRequestByEmployee(int employeeId)
            => _http.GetAsync<List<LeaveRequestViewModel>>($"{Url}/requests/employee/{employeeId}");

        public Task<ApiResult<bool>> CancelLeaveRequest(int leaveRequestId, int employeeId)
            => _http.PostAsync<bool>($"{Url}/requests/cancel/{leaveRequestId}?employeeid={employeeId}", new { });
    }

    // ===================== FestivalHoliday =====================

    public sealed class FestivalHolidayApi : BaseCrudApi<FestivalHolidayViewModel>
    {
        private const string Url = "api/FestivalHoliday";

        public FestivalHolidayApi(HttpService http)
            : base(http, Url) { }

        public Task<ApiResult<bool>> Update(int id, FestivalHolidayViewModel dto) =>
            ToBool(UpdateRaw(id, dto), "Unable to update holiday.");

        public Task<ApiResult<List<FestivalHolidayViewModel>>> GetByName(string name) =>
            Http.GetAsync<List<FestivalHolidayViewModel>>(
                $"{Url}/by-name?name={name}");

        public Task<ApiResult<List<FestivalHolidayViewModel>>> GetByMonth(int year, int month) =>
            Http.GetAsync<List<FestivalHolidayViewModel>>(
                $"{Url}/monthly?year={year}&month={month}");
    }

}
