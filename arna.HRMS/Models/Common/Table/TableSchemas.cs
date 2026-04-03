using arna.HRMS.Core.Entities;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Models.Common.Table;

public class TableSchemas
{
    // USERS TABLE
    public static List<TableColumn<UserViewModel>> Users = new()
    {
        new() { Header = "Username", Value = u => u.Username },
        new() { Header = "Email", Value = u => u.Email },
        new() { Header = "Name", Value = u => u.FullName },
        new() { Header = "Role", Value = u => u.Role },
        new() { Header = "Employee", Value = u => u.EmployeeName }
    };

    // DEPARTMENT TABLE
    public static List<TableColumn<DepartmentViewModel>> Departments = new()
    {
        new() { Header = "Name", Value = d => d.Name },
        new() { Header = "Code", Value = d => d.Code },
        new() { Header = "Description", Value = d => d.Description },
        new() { Header = "Parent Department", Value = d => d.ParentDepartMentName }
    };

    // EMPLOYEES TABLE
    public static List<TableColumn<EmployeeViewModel>> Employees = new()
    {
        new() { Header = "Number", Value = u => u.EmployeeNumber },
        new() { Header = "Name", Value = u => u.FullName },
        new() { Header = "Office Email", Value = u => u.OfficeEmail},
        new() { Header = "Department Code", Value = u => u.DepartmentCode, CssClass = "badge bg-info" },
        new() { Header = "Manager Name", Value = u => u.ManagerFullName },
        new() { Header = "Designation", Value = u => u.Position }
    };


    // ROLE TABLE
    public static List<TableColumn<RoleViewModel>> Roles = new()
    {
        new() { Header = "Name", Value = r => r.Name },
        new() { Header = "Description", Value = r => r.Description }
    };

    // LEAVE TABLE
    public static List<TableColumn<LeaveTypeViewModel>> LeaveTypes = new()
    {
        new()
        {
            Header = "Name",
            Value = u => System.Text.RegularExpressions.Regex
                            .Replace(u.LeaveNameId.ToString(), "([a-z])([A-Z])", "$1 $2")
        },
        new()
        {
            Header = "Days",
            Value = u => u.MaxPerYear.ToString()
        },
        new()
        {
            Header = "Description",
            Value = u => u.Description ?? "—"
        },
        new()
        {
            Header = "Is Paid",
            Value = u => u.IsPaid ? "Yes" : "No"
        },
    };

    public static List<TableColumn<LeaveRequestViewModel>> LeaveRequest = new()
    {
        new() { Header = "Employee Number", Value = u => u.EmployeeNumber },
        new() { Header = "Employee Name", Value = u => u.EmployeeName },
        new() { Header = "Leave Type Id", Value = u => u.LeaveTypeName },
        new() { Header = "Reason", Value = u => u.Reason },
        new() { Header = "Total Days", Value = u => u.LeaveDays },
        new() { Header = "Status", Value = u => u.StatusId },
        new() { Header = "ApprovedBy", Value = u => u.ApprovedByName },
        new() { Header = "Start Date", Value = u => u.StartDate.ToString("yyyy/MM/dd") },
        new() { Header = "End Date", Value = u => u.EndDate.Date.ToString("yyyy/MM/dd") },
    };

    public static List<TableColumn<LeaveRequestViewModel>> DashboardPendingLeaves = new()
    {
        new()
        {
            Header = "Employee",
            Value = u => $"{u.EmployeeNumber}"
        },
         new()
        {
            Header = "Employee",
            Value = u => $"{u.EmployeeName}"
        },
        new()
        {
            Header = "LeaveType Id",
            Value = u => System.Text.RegularExpressions.Regex
                            .Replace(u.LeaveTypeName ?? u.LeaveTypeId.ToString(),
                                     "([a-z])([A-Z])", "$1 $2"),
            CssClass = "badge text-center bg-light text-dark border"
        },

        new()
        {
            Header = "Duration",
            Value = u => $"{u.StartDate:MMM dd} – {u.EndDate:MMM dd}"
        },
        new()
        {
            Header = "Days",
            Value = u => u.LeaveDays.ToString(),
            CssClass = "fw-semibold"
        },
        new()
        {
            Header = "Status",
            Value = u => u.StatusId.ToString(),
            CssClassFunc = u => u.StatusId switch
            {
                Models.Enums.Status.Pending   => "badge bg-warning text-dark",
                Models.Enums.Status.Approved  => "badge bg-success",
                Models.Enums.Status.Rejected  => "badge bg-danger",
                Models.Enums.Status.Cancelled => "badge bg-secondary",
                _ => "badge bg-secondary"
            }
        }
    };

    // Add this to your existing TableSchemas class

    public static List<TableColumn<FestivalHolidayViewModel>> FestivalHolidays = new()
    {
        new()
        {
            Header = "Festival Name",
            Value = f => f.FestivalName
        },
        new()
        {
            Header = "Date",
            Value = f => f.Date.ToString("MMM dd, yyyy")
        },
        new()
        {
            Header = "Day",
            Value = f => f.Days,
            CssClass = "badge bg-light text-dark border"
        },
        new()
        {
            Header = "Description",
            Value = f => !string.IsNullOrWhiteSpace(f.Description)
                        ? (f.Description.Length > 40
                            ? f.Description[..40] + "..."
                            : f.Description)
                        : "—"
        }
    };

    public static List<TableColumn<LeaveRequestViewModel>> LeaveRequestColumns = new()
    {
        new() { Header = "Leave Type", Value = u => u.LeaveTypeName },
        new() { Header = "Reason", Value = u => string.IsNullOrWhiteSpace(u.Reason) ? "-" : u.Reason },
        new() { Header = "Total Days", Value = u => u.LeaveDays.ToString() },
        new() { Header = "Start Date", Value = u => u.StartDate.ToString("yyyy/MM/dd") },
        new() { Header = "End Date", Value = u => u.EndDate.ToString("yyyy/MM/dd") },
    };

    public static List<TableColumn<MonthlyAttendanceViewModel>> GetAttendanceColumns(int employeeId) => new()
    {
        new() { Header = "Date",           Value = u => u.Date.ToString("yyyy-MM-dd") },
        new() { Header = "Day",            Value = u => u.Day },
        new() { Header = "Clock In",       Value = u => GetEmp(u, employeeId)?.ClockIn?.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Clock Out",      Value = u => GetEmp(u, employeeId)?.ClockOut?.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Working Hours",  Value = u => GetEmp(u, employeeId)?.WorkingHours.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Break Duration", Value = u => GetEmp(u, employeeId)?.BreakDuration.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Total Hours",    Value = u => GetEmp(u, employeeId)?.TotalHours.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Status",         Value = u => GetEmp(u, employeeId)?.Status ?? "-" },
    };

    public static List<TableColumn<AttendanceRequestViewModel>> AttendanceRequest = new()
    {
        new() { Header = "Employee Name",   Value = u => u.EmployeeName },
        new() { Header = "From Date",       Value = u => u.FromDate },
        new() { Header = "To Date",         Value = u => u.ToDate },
        new() { Header = "Clock In",        Value = u => u.ClockIn?.ToString(@"hh\:mm tt") ?? "-" },
        new() { Header = "Clock Out",       Value = u => u.ClockOut?.ToString(@"hh\:mm tt") ?? "-" },
        new() { Header = "Reason",          Value = u => u.ReasonTypeId },
        new() { Header = "Status",          Value = u => u.StatusId.ToString() },
    };

    public static List<TableColumn<MonthlyAttendanceViewModel>> EmployeesAttendanceDetail => new()
    {
        new() { Header = "Emp.Number", Value = u => u.Employees.FirstOrDefault()?.EmployeeNumber},
        new() { Header = "Name", Value = u => u.Employees.FirstOrDefault()?.EmployeeName},
        new() { Header = "Date",      Value = u => u.Date.ToString("dd MMM yyyy") },
        new() { Header = "Attendance",    Value = u => u.Employees.FirstOrDefault()?.Status ?? "—" },
        new() { Header = "Clock In",  Value = u => FormatClockTime(u.Employees.FirstOrDefault()) },
        new() { Header = "Clock Out", Value = u => FormatClockOutTime(u.Employees.FirstOrDefault()) },
        new()
            {
                Header = "Break",
                Value  = u =>  {
                    var emp = u.Employees.FirstOrDefault();

                    if (emp == null || emp.Breaks == null || !emp.Breaks.Any())
                        return "--"; 

                    return emp.BreakDuration.ToString(@"hh\:mm\:ss");
                },
                TooltipHtml = u =>
                {
                    var emp = u.Employees.FirstOrDefault();
                    if (emp?.Breaks == null || !emp.Breaks.Any()) return null;

                    var rows = emp.Breaks.Select((b, i) =>
                        $"""
                        <tr>
                            <td class="pe-4">
                                {b.BreakStart:hh\:mm\:ss} – {b.BreakEnd:hh\:mm\:ss}
                            </td>
                        
                            <td class="text-end fw-semibold">
                                {b.Duration:hh\:mm\:ss}
                            </td>
                        </tr>
                        """);

                    return $"""
                    <table class="table table-sm mb-0 text-white">
                        <thead>
                            <tr class="small text-uppercase text-muted">
                                <th class="fw-semibold">Break Time</th>
                                <th class="text-end fw-semibold">Duration</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", rows)}
                        </tbody>
                    </table>
                    """;
                }
            },
        new() { Header = "Working",   Value = u => FormatEmpHours(u.Employees.FirstOrDefault(), e => e.WorkingHours) },
        new() { Header = "Total",     Value = u => FormatEmpHours(u.Employees.FirstOrDefault(), e => e.TotalHours) },
    };

    public static List<TableColumn<MonthlyAttendanceViewModel>> EmployeeAttendence = new()
    {
        new() { Header = "Date",      Value = u => u.Date.ToString("dd MMM yyyy") },
        new() { Header = "Attendance",    Value = u => u.Employees.FirstOrDefault()?.Status ?? "—" },
        new() { Header = "Clock In",  Value = u => FormatClockTime(u.Employees.FirstOrDefault()) },
        new() { Header = "Clock Out", Value = u => FormatClockOutTime(u.Employees.FirstOrDefault()) },
        new()
            {
                Header = "Break",
                Value  = u =>  {
                    var emp = u.Employees.FirstOrDefault();

                    if (emp == null || emp.Breaks == null || !emp.Breaks.Any())
                        return "-";

                    return emp.BreakDuration.ToString(@"hh\:mm\:ss");
                },
                TooltipHtml = u =>
                {
                    var emp = u.Employees.FirstOrDefault();
                    if (emp?.Breaks == null || !emp.Breaks.Any()) return null;

                    var rows = emp.Breaks.Select((b, i) =>
                        $"""
                        <tr>
                            <td class="pe-4">
                                {b.BreakStart:hh\:mm\:ss} – {b.BreakEnd:hh\:mm\:ss}
                            </td>
                        
                            <td class="text-end fw-semibold">
                                {b.Duration:hh\:mm\:ss}
                            </td>
                        </tr>
                        """);

                    return $"""
                    <table class="table table-sm mb-0 text-white">
                        <thead>
                            <tr class="small text-uppercase text-muted">
                                <th class="fw-semibold">Break Time</th>
                                <th class="text-end fw-semibold">Duration</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", rows)}
                        </tbody>
                    </table>
                    """;
                }
            },
        new() { Header = "Working",   Value = u => FormatEmpHours(u.Employees.FirstOrDefault(), e => e.WorkingHours) },
        new() { Header = "Total",     Value = u => FormatEmpHours(u.Employees.FirstOrDefault(), e => e.TotalHours) },
    };  

    private static string FormatEmpHours(EmployeeDailyAttendanceViewModel? emp,
     Func<EmployeeDailyAttendanceViewModel, TimeSpan> selector)
    {
        if (emp == null) return "--";
        var ts = selector(emp);
        if (ts == TimeSpan.Zero) return "--";
        return $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    private static string FormatClockTime(EmployeeDailyAttendanceViewModel? emp)
    {
        if (emp?.ClockIn == null) return "--";
        return emp.ClockIn.Value.ToString(@"hh\:mm\:ss");
    }

    private static string FormatClockOutTime(EmployeeDailyAttendanceViewModel? emp)
    {
        if (emp?.ClockOut == null) return "--";
        return emp.ClockOut.Value.ToString(@"hh\:mm\:ss");
    }

    public static List<TableColumn<AttendanceRequestViewModel>> EmployeeAttendenceRequestTable = new()
    {
        new() { Header = "Date Range",  Value = u => $"{u.FromDate?.ToString("dd MMM yyyy") ?? "--"} ? {u.ToDate?.ToString("dd MMM yyyy") ?? "--"}" },
        new() { Header = "Reason",      Value = u => u.ReasonTypeId?.ToString() ?? "--" },
        new() { Header = "Location",    Value = u => u.LocationId?.ToString() ?? "--" },
        new() { Header = "Clock In",    Value = u => u.ClockIn?.ToString("hh:mm tt") ?? "--" },
        new() { Header = "Clock Out",   Value = u => u.ClockOut?.ToString("hh:mm tt") ?? "--" },
        new() { Header = "Break",       Value = u => u.BreakDuration?.ToString(@"hh\:mm") ?? "00:00" },
        new() { Header = "Total",       Value = u => u.TotalHours.ToString(@"hh\:mm") },
        new() { Header = "Description", Value = u => TruncateText(u.Description, 20) },
    };

    public static List<TableColumn<AttendanceReportViewModel>> EmployeeAttendanceDetailReport = new()
    {
        new() {Header = "Employee Name" , Value = u => u.EmployeeName},
        new() { Header = "Date", Value = u => u.AttendanceDate.ToString("dd MMM yyyy") },
        new() { Header = "Day", Value = u => u.Day ?? "—" },
        new() { Header = "Clock In", Value = u => u.ClockIn?.ToString(@"hh\:mm") ?? "--" },
        new() { Header = "Clock Out", Value = u => u.ClockOut?.ToString(@"hh\:mm") ?? "--" },
        new() { Header = "Break", Value = u => u.BreakDuration?.ToString(@"hh\:mm") ?? "--" },
        new() { Header = "Working Hours", Value = u => u.WorkingHours?.ToString(@"hh\:mm") ?? "--" },
        new() { Header = "Total Hours", Value = u => u.TotalHours?.ToString(@"hh\:mm") ?? "--" },
        new() { Header = "Latitude", Value = u => u.Latitude?.ToString("F3")},
        new() { Header = "Longitude", Value = u => u.Longitude?.ToString("F3")},
        new() { Header = "Status", Value = u => u.AttendanceStatus?.ToString() ?? "—"},
        new() { Header = "Device", Value = u => u.Devices != null && u.Devices.Any()
                        ? string.Join(", ", u.Devices.Select(d => FormatDevice(d)))
                        : "—"
                }
    };
    private static string FormatDevice(DeviceType device) => device switch
    {
        DeviceType.Mobile => "Mobile",
        DeviceType.Tablet => "Tablet",
        DeviceType.LaptopDesktop => "Laptop/Desktop",
        DeviceType.UnknownDevice => "Unknown",
        _ => "Unknown"
    };

    public static List<TableColumn<EmployeeAttendanceReportViewModel>> EmployeeReport = new()
    {
        new() {Header = "Employee Number" , Value = u => u.EmployeeNumber},
        new() {Header = "Employee Name" , Value = u => u.EmployeeName},
        new() {Header  = "Total WorkDays" , Value = u => u.TotalWorkDays},
        new () {Header = "Total Present" , Value = u => u.TotalPresent},
        new () {Header = "Total Absent" , Value = u => u.TotalAbsent},
        new () {Header = "Total Hours" , Value = u => u.TotalHours},
        new () {Header = "Avg WorkHours" , Value = u => u.AvgWorkHours},
        new () {Header = "Avg Break" , Value = u => u.AvgBreak},
    };

    public static List<TableColumn<EmployeeDailyAttendanceViewModel>> PresentEmployee = new()
    {
        new() { Header = "Employee Number",       Value = x => x.EmployeeNumber },
        new() { Header = "Name",         Value = x => x.EmployeeName },
        new() { Header = "Clock In",     Value = x => x.ClockIn.HasValue ? x.ClockIn.Value.ToString(@"hh\:mm\:ss") : "--" },
        new() { Header = "Clock Out",    Value = x => x.ClockOut.HasValue ? x.ClockOut.Value.ToString(@"hh\:mm\:ss") : "--" },
        new() { Header = "Working",      Value = x => x.WorkingHours.ToString(@"hh\:mm\:ss") },
        new()
        {
            Header = "Break",
            Value  = u => u.BreakDuration.ToString(@"hh\:mm\:ss"),
            TooltipHtml = u =>
            {
                if (!u.Breaks.Any()) return null;

                var rows = u.Breaks.Select(b =>
                    $"""
                    <tr>
                        <td class="pe-4">
                            {b.BreakStart:hh\:mm\:ss} – {b.BreakEnd:hh\:mm\:ss}
                        </td>

                        <td class="text-end fw-semibold">
                            {b.Duration:hh\:mm\:ss}
                        </td>
                    </tr>
                    """);

                return $"""
                    <table class="table table-sm mb-0 text-white">
                        <thead>
                            <tr class="small text-uppercase text-muted">
                                <th class="fw-semibold">Break Time</th>
                                <th class="text-end fw-semibold">Duration</th>
                            </tr>
                        </thead>
                        <tbody>
                            {string.Join("", rows)}
                        </tbody>
                    </table>
                    """;

            }
        },
        new() { Header = "Total",        Value = x => x.TotalHours.ToString(@"hh\:mm\:ss") },
    };


    private static string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return "--";
        return text.Length <= maxLength ? text : $"{text[..maxLength]}…";
    }

    private static EmployeeDailyAttendanceViewModel? GetEmp(MonthlyAttendanceViewModel day, int empId)
        => day.Employees.FirstOrDefault(e => e.EmployeeId == empId);
}
