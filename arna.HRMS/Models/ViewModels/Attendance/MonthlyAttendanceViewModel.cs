using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.ViewModels.Attendance;

public class MonthlyAttendanceViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public string? Status { get; set; }
}
