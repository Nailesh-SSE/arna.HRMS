using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Common;

public class ApiClients
{
    public ApiEndpoint<EmployeeDto> Employees { get; }
    public ApiEndpoint<DepartmentDto> Departments { get; }
    public ApiEndpoint<AttendanceDto> Attendance { get; }

    public ApiClients(HttpService http)
    {
        Employees = new ApiEndpoint<EmployeeDto>(http, "api/employees");
        Departments = new ApiEndpoint<DepartmentDto>(http, "api/department");
        Attendance = new ApiEndpoint<AttendanceDto>(http, "api/attendance");
    }
}
