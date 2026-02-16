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
    StatusColumn<LeaveTypeViewModel>(u => u.IsActive)
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
