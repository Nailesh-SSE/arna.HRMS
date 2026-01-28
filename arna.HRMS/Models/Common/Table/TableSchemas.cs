using arna.HRMS.Models.ViewModels;
using arna.HRMS.Models.ViewModels.Attendance;

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
        new() { Header = "Employee", Value = u => u.EmployeeName },
        StatusColumn<UserViewModel>(u => u.IsActive)
    };

    // DEPARTMENT TABLE
    public static List<TableColumn<DepartmentViewModel>> Departments = new()
    {
        new() { Header = "Name", Value = d => d.Name },
        new() { Header = "Code", Value = d => d.Code },
        new() { Header = "Description", Value = d => d.Description },
        new() { Header = "Parent Department", Value = d => d.ParentDepartMentName },
        StatusColumn<DepartmentViewModel>(d => d.IsActive)
    };

    // EMPLOYEES TABLE
    public static List<TableColumn<EmployeeViewModel>> Employees = new()
    {
        new() { Header = "Number", Value = u => u.EmployeeNumber },
        new() { Header = "Full Name", Value = u => u.FullName },
        new() { Header = "Email", Value = u => u.Email },
        new() { Header = "Department Code", Value = u => u.DepartmentCode, CssClass = "badge bg-info" },
        new() { Header = "Manager Name", Value = u => u.ManagerFullName },
        new() { Header = "Position", Value = u => u.Position },
        StatusColumn<EmployeeViewModel>(u => u.IsActive)
    };

    // ATTENDANCE TABLE
    public static List<TableColumn<MonthlyAttendanceViewModel>> AttendanceList = new()
    {
        new() { Header = "Date", Value = u => u.Date.ToString("dd MMM yyyy") },
        new() { Header = "Day", Value = u => u.Day },
        new() { Header = "Clock In", Value = u => u.ClockIn?.ToString(@"hh\:mm\:ss") ?? "-" },
        new() { Header = "Clock Out", Value = u => u.ClockOut?.ToString(@"hh\:mm\:ss") ?? "-" },
        new() {Header = "Working Hours", Value = u =>u.ClockIn.HasValue && u.ClockOut.HasValue ? u.WorkingHours.Value.ToString(@"hh\:mm\:ss") : "-"},
        new() {Header = "Break Duration", Value = u => u.ClockIn.HasValue && u.ClockOut.HasValue ? u.BreakDuration.Value.ToString(@"hh\:mm\:ss") : "-"},
        new() {Header = "Total Hours", Value = u => u.ClockIn.HasValue && u.ClockOut.HasValue ? u.TotalHours.Value.ToString(@"hh\:mm\:ss") : "-"},

         new()
         {
              Header = "Status",
              Value = u => u.Status,
              CssClassFunc = u => u.Status switch
              {
                   "Present"  => "badge bg-success",
                   "Absent"   => "badge bg-danger",
                   "Holiday"  => "badge bg-info text-dark",
                   "Leave"    => "badge bg-warning text-dark",
                   "Late"     => "badge bg-info text-dark",
                   "Half-Day" => "badge bg-primary",
                   _          => "badge bg-light text-dark"
              }
         }
    };

    // ROLE TABLE
    public static List<TableColumn<RoleViewModel>> Roles = new()
    {
        new() { Header = "Name", Value = r => r.Name },
        new() { Header = "Description", Value = r => r.Description }
    };

    // GENERIC STATUS COLUMN
    private static TableColumn<T> StatusColumn<T>(Func<T, bool> isActiveSelector)
    {
        return new TableColumn<T>
        {
            Header = "Status",
            Value = item => GetStatusText(isActiveSelector(item)),
            CssClassFunc = item => GetStatusCss(isActiveSelector(item)) 
        };
    }

    private static string GetStatusText(bool isActive) => isActive ? "Active" : "Inactive";
    private static string GetStatusCss(bool isActive) => isActive ? "badge bg-success" : "badge bg-danger";
}
