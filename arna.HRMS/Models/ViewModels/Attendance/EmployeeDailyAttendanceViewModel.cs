namespace arna.HRMS.Models.ViewModels.Attendance;

public class EmployeeDailyAttendanceViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string? Status { get; set; }
}
