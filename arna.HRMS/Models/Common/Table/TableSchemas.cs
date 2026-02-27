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
        new() { Header = "Full Name", Value = u => u.FullName },
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
        new() { Header = "Full Name", Value = u => u.FullName },
        new() { Header = "Email", Value = u => u.Email },
        new() { Header = "Department Code", Value = u => u.DepartmentCode, CssClass = "badge bg-info" },
        new() { Header = "Manager Name", Value = u => u.ManagerFullName },
        new() { Header = "Position", Value = u => u.Position }
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
        new() { Header = "EmployeeNumber", Value = u => u.EmployeeNumber },
        new() { Header = "EmployeeName", Value = u => u.EmployeeName },
        new() { Header = "LeaveTypeId", Value = u => u.LeaveTypeId },
        new() { Header = "Reason", Value = u => u.Reason },
        new() { Header = "TotalDays", Value = u => u.LeaveDays },
        new() { Header = "Status", Value = u => u.StatusId },
        new() { Header = "ApprovedBy", Value = u => u.ApprovedBy },
        new() { Header = "StartDate", Value = u => u.StartDate.ToString("yyyy/MM/dd") },
        new() { Header = "EndDate", Value = u => u.EndDate.Date.ToString("yyyy/MM/dd") },
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
        new() { Header = "Clock In",       Value = u => GetEmp(u, employeeId)?.ClockIn?.ToString(@"hh\:mm") ?? "-" },
        new() { Header = "Clock Out",      Value = u => GetEmp(u, employeeId)?.ClockOut?.ToString(@"hh\:mm") ?? "-" },
        new() { Header = "Working Hours",  Value = u => GetEmp(u, employeeId)?.WorkingHours.ToString(@"hh\:mm") ?? "-" },
        new() { Header = "Break Duration", Value = u => GetEmp(u, employeeId)?.BreakDuration.ToString(@"hh\:mm") ?? "-" },
        new() { Header = "Total Hours",    Value = u => GetEmp(u, employeeId)?.TotalHours.ToString(@"hh\:mm") ?? "-" },
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

    public static List<TableColumn<EmployeeViewModel>> EmployeeAttendanceOverview => new()
{
    new ()
    {
        Header = "Employee Name",
        Value = e => e.FullName
    },
    new ()
    {
        Header = "Emp. Number",
        Value = e => e.EmployeeNumber
    },
    new ()
    {
        Header = "Department",
        Value = e => e.DepartmentName ?? "—"
    },
    new ()
    {
        Header = "Position",
        Value = e => e.Position
    },
    new()
    {
        Header = "Designation",
        Value = e => e.Designation,
    }
};
    public static List<TableColumn<MonthlyAttendanceViewModel>> EmployeeAttendence = new()
{
    new() { Header = "Date",      Value = u => u.Date.ToString("dd MMM yyyy") },
    new() { Header = "Day",       Value = u => u.Day },
    new() { Header = "Clock In",  Value = u => u.Employees.FirstOrDefault()?.ClockIn?.ToString(@"hh\:mm") ?? "--" },
    new() { Header = "Clock Out", Value = u => u.Employees.FirstOrDefault()?.ClockOut?.ToString(@"hh\:mm") ?? "--" },
    new() { Header = "Break",     Value = u => u.Employees.FirstOrDefault()?.BreakDuration.ToString(@"hh\:mm") ?? "--" },
    new() { Header = "Working",   Value = u => u.Employees.FirstOrDefault()?.WorkingHours.ToString(@"hh\:mm") ?? "--" },
    new() { Header = "Total",     Value = u => u.Employees.FirstOrDefault()?.TotalHours.ToString(@"hh\:mm") ?? "--" },
    new() { Header = "Status",    Value = u => u.Employees.FirstOrDefault()?.Status ?? "—" },
};

    public static List<TableColumn<AttendanceRequestViewModel>> EmployeeAttendenceRequestTable = new()
{
    new() { Header = "Date Range",  Value = u => $"{u.FromDate?.ToString("dd MMM yyyy") ?? "--"} → {u.ToDate?.ToString("dd MMM yyyy") ?? "--"}" },
    new() { Header = "Reason",      Value = u => u.ReasonTypeId?.ToString() ?? "--" },
    new() { Header = "Location",    Value = u => u.LocationId?.ToString() ?? "--" },
    new() { Header = "Clock In",    Value = u => u.ClockIn?.ToString("hh:mm tt") ?? "--" },
    new() { Header = "Clock Out",   Value = u => u.ClockOut?.ToString("hh:mm tt") ?? "--" },
    new() { Header = "Break",       Value = u => u.BreakDuration?.ToString(@"hh\:mm") ?? "00:00" },
    new() { Header = "Total",       Value = u => u.TotalHours.ToString(@"hh\:mm") },
    new() { Header = "Description", Value = u => TruncateText(u.Description, 30) },
};

    private static string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text)) return "--";
        return text.Length <= maxLength ? text : $"{text[..maxLength]}…";
    }
    private static EmployeeDailyAttendanceViewModel? GetEmp(MonthlyAttendanceViewModel day, int empId)
        => day.Employees.FirstOrDefault(e => e.EmployeeId == empId);
}
