namespace arna.HRMS.Models.ViewModels.Attendance;

public class EmployeeDailyAttendanceViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public List<BreakViewModel> Breaks { get; set; } = new();
    public TimeSpan BreakDuration => TimeSpan.FromSeconds(Breaks.Sum(b => b.Duration.TotalSeconds));
    public TimeSpan TotalHours { get; set; }
    public string? Status { get; set; }
}
