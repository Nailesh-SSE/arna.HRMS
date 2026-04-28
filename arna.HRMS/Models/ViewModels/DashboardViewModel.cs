using arna.HRMS.Core.DTOs;
using arna.HRMS.Models.ViewModels.Attendance;
using arna.HRMS.Models.ViewModels.Leave;

namespace arna.HRMS.Models.ViewModels;

public class DashboardViewModel
{
    public List<EmployeeViewModel> Employees { get; set; } = new();
    public List<LeaveTypeViewModel> LeaveTypes { get; set; } = new();
    public List<AttendanceRequestViewModel> AttendanceRequests { get; set; } = new();
    public List<EmployeeDailyAttendanceViewModel> Attendance { get; set; } = new();
    public List<EmployeeDailyAttendanceViewModel> LeaveEmployees { get; set; } = new();
    public List<LeaveRequestViewModel> Requests { get; set; } = new();

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
