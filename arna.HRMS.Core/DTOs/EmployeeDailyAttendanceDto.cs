namespace arna.HRMS.Core.DTOs;

public class EmployeeDailyAttendanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public List<BreakDto> Breaks { get; set; } = new();
    public TimeSpan BreakDuration => TimeSpan.FromSeconds(Breaks.Sum(b => b.Duration.TotalSeconds));
    public TimeSpan TotalHours { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
}

