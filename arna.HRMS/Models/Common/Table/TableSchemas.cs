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
    public static List<TableColumn<LeaveMasterViewModel>> LeaveMasters = new() 
    {
        new() { Header = "Name", Value = u => u.LeaveName },
        new() { Header = "Days", Value = u => u.MaxPerYear },
        new() { Header = "Description", Value = u => u.Description },
        new() {Header = "IsPaid", Value = u => u.IsPaid},
        StatusColumn<LeaveMasterViewModel>(u => u.IsActive)
    };

    public static List<TableColumn<LeaveRequestViewModel>> LeaveRequest = new()
    {
        new() { Header = "EmployeeNumber", Value = u => u.EmployeeNumber },
        new() { Header = "EmployeeName", Value = u => u.EmployeeName },
        new() { Header = "LeaveTypeId", Value = u => u.LeaveTypeId },
        new() { Header = "Reason", Value = u => u.Reason },
        new() { Header = "TotalDays", Value = u => u.TotalDays },
        new() { Header = "Status", Value = u => u.StatusId },
        new() { Header = "ApprovedBy", Value = u => u.ApprovedBy },
        new() { Header = "StartDate", Value = u => u.StartDate.ToString("yyyy/MM/dd") },
        new() { Header = "EndDate", Value = u => u.EndDate.Date.ToString("yyyy/MM/dd") },
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
