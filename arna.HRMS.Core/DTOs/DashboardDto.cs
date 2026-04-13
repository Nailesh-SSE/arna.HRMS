namespace arna.HRMS.Core.DTOs;

public class DashboardDto
{
    public List<EmployeeDto> Employees { get; set; } = new();
    public List<LeaveTypeDto> LeaveTypes { get; set; } = new();
    public List<AttendanceRequestDto> AttendanceRequests { get; set; } = new();
    public List<EmployeeDailyAttendanceDto> Attendance { get; set; } = new();
    public List<EmployeeDailyAttendanceDto> LeaveEmployees { get; set; } = new();
    public List<LeaveRequestDto> Requests { get; set; } = new();

    public int AttendanceTotalRequests { get; set; }
    public int AttendancePendingRequests { get; set; }
    public int LeaveTotalRequests { get; set; }
    public int LeavePendingRequests { get; set; }
    public int LeaveApprovedRequests { get; set; }
    public int LeaveRejectedRequests { get; set; }
    public int LeaveCancledRequests { get; set; }
    public int PresentEmployee { get; set; }
    public int AbsentEmployee { get; set; }
    public int TotalEmployees { get; set; }
    public int AvgWorkingHours { get; set; }
    public int AvgBreakHours { get; set; }
}