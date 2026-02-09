namespace arna.HRMS.Core.DTOs;

public class EmployeeDailyAttendanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan WorkingHours { get; set; }
    public TimeSpan BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string? Status { get; set; }
}

