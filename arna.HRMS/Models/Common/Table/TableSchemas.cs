using arna.HRMS.Models.DTOs;

namespace arna.HRMS.Models.Common.Table;

public class TableSchemas
{
    // USERS TABLE
    public static List<TableColumn<UserDto>> Users = new()
    {
        new() { Header = "Username", Value = u => u.Username },
        new() { Header = "Email", Value = u => u.Email },
        new() { Header = "Full Name", Value = u => u.FullName },
        new() { Header = "Phone", Value = u => u.PhoneNumber },
        new() { Header = "Role", Value = u => u.Role },
        new() { Header = "Employee", Value = u => u.EmployeeName },
        StatusColumn<UserDto>(u => u.IsActive)
    };

    // DEPARTMENT TABLE
    public static List<TableColumn<DepartmentDto>> Departments = new()
    {
        new() { Header = "Name", Value = d => d.Name },
        new() { Header = "Code", Value = d => d.Code },
        new() { Header = "Description", Value = d => d.Description },
        new() { Header = "Parent Department", Value = d => d.ParentDepartMentName },
        StatusColumn<DepartmentDto>(d => d.IsActive)
    };

    // EMPLOYEES TABLE
    public static List<TableColumn<EmployeeDto>> Employees = new()
    {
        new() { Header = "Number", Value = u => u.EmployeeNumber },
        new() { Header = "Full Name", Value = u => u.FullName },
        new() { Header = "Email", Value = u => u.Email },
        new() { Header = "Department Code", Value = u => u.DepartmentCode, CssClass = "badge bg-info" },
        new() { Header = "Manager Name", Value = u => u.ManagerFullName },
        new() { Header = "Position", Value = u => u.Position },
        StatusColumn<EmployeeDto>(u => u.IsActive)
    };

    public static List<TableColumn<MonthlyAttendanceDto>> AttendanceList = new()
{
    new() { Header = "Id", Value = u => u.EmployeeId },
    new() { Header = "Date", Value = u => u.Date.ToString("dd MMM yyyy") },
    new() { Header = "Day", Value = u => u.Day },
    new() { Header = "Clock In", Value = u => u.ClockIn?.ToString(@"hh\:mm") ?? "-" },
    new() { Header = "Clock Out", Value = u => u.ClockOut?.ToString(@"hh\:mm") ?? "-" },
    new() { Header = "Total Hours", Value = u => u.TotalHours.ToString(@"hh\:mm") },

     new()
    {
        Header = "Status",
        Value = u => u.Status,
        CssClassFunc = u => u.Status switch
        {
            "Present"  => "badge bg-success",
            "Absent"   => "badge bg-danger",
            "Holiday"  => "badge bg-secondary",
            "Leave"    => "badge bg-warning text-dark",
            "Late"     => "badge bg-info text-dark",
            "Half-Day" => "badge bg-primary",
            _          => "badge bg-light text-dark"
        }
    }
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
