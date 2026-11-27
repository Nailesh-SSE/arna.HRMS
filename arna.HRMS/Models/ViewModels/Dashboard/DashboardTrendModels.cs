namespace arna.HRMS.Models.ViewModels.Dashboard;

public class DepartmentAttendanceSummary
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalStaff { get; set; }
    public int PresentCount { get; set; }
    public double AttendanceRate => TotalStaff > 0 ? Math.Round((double)PresentCount / TotalStaff * 100.0, 1) : 0;
    public string ProgressColorClass => AttendanceRate >= 90 ? "bg-success" : (AttendanceRate >= 75 ? "bg-primary" : "bg-warning");
}

public class DailyAttendanceTrendPoint
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public double AttendancePercent { get; set; }
    public int PresentCount { get; set; }
    public int TotalStaff { get; set; }
    public bool IsWeekend { get; set; }
    public bool IsFuture { get; set; }
    public double SvgX { get; set; }
    public double SvgY { get; set; }
    public string CustomTooltip { get; set; } = string.Empty;

    public string FullTooltip => !string.IsNullOrWhiteSpace(CustomTooltip)
        ? CustomTooltip
        : $"{DayLabel} ({Date:MMM dd, yyyy}): {AttendancePercent:F1}% rate ({PresentCount} of {TotalStaff} Present)";
}
