using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Leave;
using Microsoft.AspNetCore.Components;

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
        new() { Header = "Employee", Value = u => u.EmployeeName },
        new()
        {
            Header = "STATUS",
            Value = u => u.IsActive ? "Active" : "Inactive",
            CssClassFunc = u => u.IsActive ? "badge bg-success" : "badge bg-danger"
        }
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
        new() { Header = "Emp#", Value = u => u.EmployeeNumber },
        new() { Header = "Name", Value = u => u.FullName },
        new() { Header = "Office Email", Value = u => u.OfficeEmail},
        new() { Header = "Dep. Code", Value = u => u.DepartmentCode, CssClass = "badge bg-info" },
        new() { Header = "Manager Name", Value = u => u.ManagerFullName },
        new() { Header = "Designation", Value = u => u.Position },
        new()
        {
            Header = "STATUS",
            Value = u => u.IsActive ? "Active" : "Inactive",
            CssClassFunc = u => u.IsActive ? "badge bg-success" : "badge bg-danger"
        }
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
            Value = u => u.Description ?? "�"
        },
        new()
        {
            Header = "Is Paid",
            Value = u => u.IsPaid ? "Yes" : "No"
        },
    };

    public static List<TableColumn<LeaveRequestViewModel>> LeaveRequest = new()
    {
        new() { Header = "Emp#", Value = u => u.EmployeeNumber },
        new() { Header = "Employee Name", Value = u => u.EmployeeName },
        new() { Header = "Leave Type Id", Value = u => u.LeaveTypeName },
        new () {Header = "Reason" , Value = u => string.IsNullOrWhiteSpace(u.Reason) ? "-" : u.Reason},
        new() { Header = "Total Days", Value = u => u.LeaveDays },
        new() { Header = "Status", Value = u => u.StatusId },
        new() { Header = "ApprovedBy", Value = u => u.ApprovedByName },
        new() { Header = "Start Date", Value = u => u.StartDate.ToString("dd MMM yyyy") },
        new() { Header = "End Date", Value = u => u.EndDate.Date.ToString("dd MMM yyyy") },
    };

    public static List<TableColumn<LeaveRequestViewModel>> DashboardPendingLeaves = new()
    {
        new()
        {
            Header = "Emp#",
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
            Value = u => $"{u.StartDate:MMM dd} � {u.EndDate:MMM dd}"
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
            Value = f => f.Date.ToString("dd MMM yyyy")
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
                        : "�"
        }
    };

    public static List<TableColumn<LeaveRequestViewModel>> LeaveRequestColumns = new()
    {
        new() { Header = "Leave Type", Value = u => u.LeaveTypeName },
        new () {Header = "Reason" , Value = u => string.IsNullOrWhiteSpace(u.Reason) ? "-" : u.Reason},
        new() { Header = "Total Days", Value = u => u.LeaveDays.ToString() },
        new() { Header = "Start Date", Value = u => u.StartDate.ToString("dd MMM yyyy") },
        new() { Header = "End Date", Value = u => u.EndDate.ToString("dd MMM yyyy") },
    };

    public static List<TableColumn<MonthlyAttendanceViewModel>> GetAttendanceColumns(int employeeId) => new()
    {
        new() { Header = "Date",           Value = u => u.Date.ToString("dd MMM yyyy") },
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
        new() { Header = "From Date",       Value = u => u.FromDate?.ToString("dd MMM yyyy") },
        new() { Header = "To Date",         Value = u => u.ToDate?.ToString("dd MMM yyyy") },
        new() { Header = "Clock In",        Value = u => u.ClockIn?.ToString(@"hh\:mm tt") ?? "-" },
        new() { Header = "Clock Out",       Value = u => u.ClockOut?.ToString(@"hh\:mm tt") ?? "-" },
        new() { Header = "Reason",          Value = u => u.ReasonTypeId?.ToString() ?? "-" },
        new() { Header = "Status",          Value = u => u.StatusId.ToString() },
    };

    public static List<TableColumn<MonthlyAttendanceViewModel>> EmployeesAttendanceDetail => new()
    {
        new() { Header = "Emp#", Value = u => u.Employees.FirstOrDefault()?.EmployeeNumber},
        new() { Header = "Name", Value = u => u.Employees.FirstOrDefault()?.EmployeeName},
        new() { Header = "Date",      Value = u => u.Date.ToString("dd MMM yyyy") },
        new() { Header = "Attendance",    Value = u => u.Employees.FirstOrDefault()?.Status ?? "�" },
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
                                {b.BreakStart:hh\:mm\:ss} � {b.BreakEnd:hh\:mm\:ss}
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
        new() { Header = "Attendance",    Value = u => u.Employees.FirstOrDefault()?.Status ?? "�" },
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
                                {b.BreakStart:hh\:mm\:ss} � {b.BreakEnd:hh\:mm\:ss}
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
         new() { Header = "Employee", Value = u => $"{u.EmployeeName}" },
        new() { Header = "Date Range",   Value = u =>
        {
            if (u.FromDate == null) return "--";

            var from = u.FromDate.Value;
            var to = u.ToDate;

            if (from != to)
            {
                return $"{from:dd}-{to.Value:dd MMM yyyy}";
            }
            else
            {
                return from.ToString("dd MMM yyyy");
            }
        }},
        new() { Header = "Reason",       Value = u => u.ReasonTypeId?.ToString() ?? "-" },
        new() { Header = "Location",    Value = u => u.LocationId?.ToString() ?? "--" },
        new() { Header = "Clock In",    Value = u => u.ClockIn?.ToString("hh:mm tt") ?? "--" },
        new() { Header = "Clock Out",   Value = u => u.ClockOut?.ToString("hh:mm tt") ?? "--" },
        new() { Header = "Break",       Value = u => u.BreakDuration?.ToString(@"hh\:mm") ?? "00:00" },
        new() { Header = "Total",       Value = u => u.TotalHours.ToString(@"hh\:mm") },
        new() { Header = "Description", Value = u => TruncateText(u.Description, 20) },
    };

    public static List<TableColumn<AttendanceReportViewModel>> EmployeeAttendanceDetailReport = new()
    {
        new() { Header = "Date", Value = u => u.AttendanceDate.ToString("dd MMM yyyy") },
        new() { Header = "Day", Value = u => u.Day ?? u.AttendanceDate.ToString("ddd") },
        new()
        {
            Header = "Status",
            Value = u => u.AttendanceStatus switch
            {
                AttendanceStatus.Present => new MarkupString(
                    "<span class=\"badge bg-success bg-opacity-10 text-success border border-success border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-check-circle-fill\"></i>Present</span>"),
                AttendanceStatus.Absent => new MarkupString(
                    "<span class=\"badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-x-circle-fill\"></i>Absent</span>"),
                AttendanceStatus.Late => new MarkupString(
                    "<span class=\"badge bg-warning bg-opacity-10 text-warning-emphasis border border-warning border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-clock-fill\"></i>Late</span>"),
                AttendanceStatus.HalfDay => new MarkupString(
                    "<span class=\"badge bg-info bg-opacity-10 text-info-emphasis border border-info border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-hourglass-split\"></i>Half Day</span>"),
                AttendanceStatus.OnLeave => new MarkupString(
                    "<span class=\"badge bg-secondary bg-opacity-10 text-secondary border border-secondary border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-calendar2-x-fill\"></i>On Leave</span>"),
                AttendanceStatus.Holiday => new MarkupString(
                    "<span class=\"badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-calendar-event-fill\"></i>Holiday</span>"),
                AttendanceStatus.WeeklyOff => new MarkupString(
                    "<span class=\"badge bg-light text-muted border rounded-pill px-2 py-1 d-inline-flex align-items-center gap-1\"><i class=\"bi bi-calendar-minus\"></i>Weekly Off</span>"),
                _ => new MarkupString("<span class=\"text-muted small\">--</span>")
            }
        },
        new()
        {
            Header = "Clock In",
            Value = u => (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || !u.ClockIn.HasValue)
                ? "--"
                : DateTime.Today.Add(u.ClockIn.Value).ToString("hh:mm tt")
        },
        new()
        {
            Header = "Clock Out",
            Value = u => (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || !u.ClockOut.HasValue)
                ? "--"
                : DateTime.Today.Add(u.ClockOut.Value).ToString("hh:mm tt")
        },
        new()
        {
            Header = "Working Hours",
            Value = u => (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || u.AttendanceStatus == AttendanceStatus.Holiday || !u.WorkingHours.HasValue || u.WorkingHours.Value == TimeSpan.Zero)
                ? "--"
                : u.WorkingHours.Value.ToString(@"hh\:mm")
        },
        new()
        {
            Header = "Break",
            Value = u => (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || !u.BreakDuration.HasValue || u.BreakDuration.Value == TimeSpan.Zero)
                ? "--"
                : u.BreakDuration.Value.ToString(@"hh\:mm")
        },
        new()
        {
            Header = "Total Hours",
            Value = u => (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || !u.TotalHours.HasValue || u.TotalHours.Value == TimeSpan.Zero)
                ? "--"
                : u.TotalHours.Value.ToString(@"hh\:mm")
        },
        new()
        {
            Header = "Location",
            Value = u =>
            {
                if (u.AttendanceStatus != AttendanceStatus.Absent && u.AttendanceStatus != AttendanceStatus.OnLeave &&
                    u.Latitude.HasValue && u.Longitude.HasValue &&
                    (Math.Abs(u.Latitude.Value) > 0.0001 || Math.Abs(u.Longitude.Value) > 0.0001))
                {
                    var lat = u.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var lon = u.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return new MarkupString(
                        $"<a href=\"https://www.google.com/maps?q={lat},{lon}\" " +
                        $"target=\"_blank\" rel=\"noopener noreferrer\" " +
                        $"class=\"btn btn-sm btn-light border border-primary-subtle text-primary rounded-pill py-0 px-2 d-inline-flex align-items-center gap-1 text-decoration-none shadow-none\" " +
                        $"title=\"GPS: {lat}, {lon} - Click to open Google Maps\">" +
                        $"<i class=\"bi bi-geo-alt-fill text-danger\"></i><span class=\"fw-medium\">View Map</span></a>");
                }
                return new MarkupString("<span class=\"text-muted small\">--</span>");
            }
        },
        new()
        {
            Header = "Device",
            Value = u =>
            {
                if (u.AttendanceStatus == AttendanceStatus.Absent || u.AttendanceStatus == AttendanceStatus.OnLeave || u.Devices == null || !u.Devices.Any())
                    return new MarkupString("<span class=\"text-muted small\">--</span>");
                var badges = u.Devices.Select(d => d switch
                {
                    DeviceType.Mobile => "<span class=\"badge bg-light text-secondary border rounded-pill px-2 py-1\"><i class=\"bi bi-phone me-1 text-primary\"></i>Mobile</span>",
                    DeviceType.Tablet => "<span class=\"badge bg-light text-secondary border rounded-pill px-2 py-1\"><i class=\"bi bi-tablet me-1 text-info\"></i>Tablet</span>",
                    DeviceType.LaptopDesktop => "<span class=\"badge bg-light text-secondary border rounded-pill px-2 py-1\"><i class=\"bi bi-laptop me-1 text-secondary\"></i>Desktop</span>",
                    _ => "<span class=\"badge bg-light text-muted border rounded-pill px-2 py-1\">Unknown</span>"
                });
                return new MarkupString(string.Join(" ", badges));
            }
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
        new() { Header = "Emp #", Value = u => u.EmployeeNumber, CssClass = "fw-medium" },
        new() { Header = "Employee Name", Value = u => u.EmployeeName, CssClass = "fw-semibold text-dark" },
        new() { Header = "Work Days", Value = u => u.TotalWorkDays },
        new() { Header = "Present", Value = u => u.TotalPresent, CssClass = "text-success fw-semibold" },
        new() { Header = "Absent", Value = u => u.TotalAbsent, CssClass = "text-danger fw-semibold" },
        new() { Header = "Total Hours", Value = u => u.TotalHours },
        new() { Header = "Avg Work Hours", Value = u => u.AvgWorkHours },
        new() { Header = "Avg Break", Value = u => u.AvgBreak },
    };

    public static List<TableColumn<EmployeeDailyAttendanceViewModel>> PresentEmployee = new()
    {
        new() { Header = "Emp #", Value = x => x.EmployeeNumber, CssClass = "font-monospace text-secondary small" },
        new() { Header = "Name", Value = x => x.EmployeeName, CssClass = "fw-semibold text-dark" },
        new() { Header = "Clock In", Value = x => x.ClockIn.HasValue ? x.ClockIn.Value.ToString(@"hh\:mm\:ss") : "--", CssClass = "text-success fw-medium font-monospace small" },
        new() { Header = "Clock Out", Value = x => x.ClockOut.HasValue ? x.ClockOut.Value.ToString(@"hh\:mm\:ss") : "--", CssClass = "text-muted font-monospace small" },
        new() { Header = "Working", Value = x => x.WorkingHours.ToString(@"hh\:mm\:ss"), CssClass = "fw-semibold font-monospace small" },
        new()
        {
            Header = "Break",
            Value = u => u.BreakDuration.ToString(@"hh\:mm\:ss"),
            CssClass = "font-monospace small text-secondary",
            TooltipHtml = u =>
            {
                if (!u.Breaks.Any()) return null;

                var rows = u.Breaks.Select(b =>
                    $"""
                    <tr>
                        <td class="pe-4">
                            {b.BreakStart:hh\:mm\:ss} - {b.BreakEnd:hh\:mm\:ss}
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
        new() { Header = "Total", Value = x => x.TotalHours.ToString(@"hh\:mm\:ss"), CssClass = "fw-bold font-monospace small text-primary" },
    };

    public static List<TableColumn<LeaveSummaryReportViewModel>> LeaveSummaryReport = new()
    {
        new() { Header = "Emp #", Value = u => u.EmployeeNumber, CssClass = "fw-medium" },
        new() { Header = "Employee Name", Value = u => u.EmployeeName, CssClass = "fw-semibold text-dark" },
        new()
        {
            Header = "Leave Type",
            Value = u => System.Text.RegularExpressions.Regex.Replace(((LeaveName)u.LeaveNameId).ToString(), "([a-z])([A-Z])", "$1 $2"),
            CssClass = "badge bg-primary bg-opacity-10 text-primary"
        },
        new() { Header = "Annual Quota", Value = u => $"{u.MaxPerYear} days" },
        new() { Header = "Used Leave", Value = u => $"{u.UsedLeave} days", CssClass = "text-danger fw-semibold" },
        new() { Header = "Balance", Value = u => $"{Math.Max(0, u.MaxPerYear - u.UsedLeave)} days", CssClass = "text-success fw-semibold" },
    };

    public static List<TableColumn<EmployeeLeaveDetailsReportViewModel>> EmployeeLeaveDetailsReport = new()
    {
        new() { Header = "Emp #", Value = u => u.EmployeeNumber, CssClass = "fw-medium" },
        new() { Header = "Employee Name", Value = u => u.EmployeeName, CssClass = "fw-semibold text-dark" },
        new()
        {
            Header = "Leave Type",
            Value = u => u.LeaveNameId.HasValue
                ? System.Text.RegularExpressions.Regex.Replace(((LeaveName)u.LeaveNameId.Value).ToString(), "([a-z])([A-Z])", "$1 $2")
                : "-",
            CssClass = "badge bg-primary bg-opacity-10 text-primary"
        },
        new() { Header = "Reason", Value = u => string.IsNullOrWhiteSpace(u.Reason) ? "-" : u.Reason },
        new() { Header = "Start Date", Value = u => u.StartDate.HasValue ? u.StartDate.Value.ToString("dd MMM yyyy") : "-" },
        new() { Header = "End Date", Value = u => u.EndDate.HasValue ? u.EndDate.Value.ToString("dd MMM yyyy") : "-" },
        new() { Header = "Total Days", Value = u => $"{u.LeaveDays} days", CssClass = "fw-semibold" },
        new()
        {
            Header = "Status",
            Value = u => u.StatusId.HasValue ? ((Status)u.StatusId.Value).ToString() : "-",
            CssClassFunc = u => u.StatusId switch
            {
                (int)Status.Pending => "badge bg-warning bg-opacity-15 text-warning fw-semibold",
                (int)Status.Approved => "badge bg-success bg-opacity-15 text-success fw-semibold",
                (int)Status.Rejected => "badge bg-danger bg-opacity-15 text-danger fw-semibold",
                _ => "badge bg-secondary"
            }
        },
    };

    public static List<TableColumn<EmployeeDailyAttendanceViewModel>> LeaveEmployee = new()
    {
        new() { Header = "Emp #", Value = x => x.EmployeeNumber, CssClass = "font-monospace text-secondary small" },
        new() { Header = "Name", Value = x => x.EmployeeName, CssClass = "fw-semibold text-dark" },
        new() { Header = "Reason", Value = x => string.IsNullOrWhiteSpace(x.Note) ? "Approved Leave" : x.Note, CssClass = "text-secondary small" },
        new()
        {
            Header = "Status",
            Value = x => string.IsNullOrWhiteSpace(x.Status) ? "On Leave" : x.Status,
            CssClassFunc = x => (x.Status ?? "").Contains("Leave", StringComparison.OrdinalIgnoreCase)
                ? "badge bg-success bg-opacity-15 text-success fw-semibold px-2.5 py-1"
                : "badge bg-warning bg-opacity-25 text-warning fw-semibold px-2.5 py-1"
        },
    };


    private static string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return "--";
        return text.Length <= maxLength ? text : $"{text[..maxLength]}�";
    }

    private static EmployeeDailyAttendanceViewModel? GetEmp(MonthlyAttendanceViewModel day, int empId)
        => day.Employees.FirstOrDefault(e => e.EmployeeId == empId);
}
