namespace arna.HRMS.Models.ViewModels;
public class EmployeeAttendanceReportViewModel
{
    public int EmployeeId { get; set; }

    public string EmployeeNumber { get; set; }

    public string EmployeeName { get; set; }

    public int TotalWorkDays { get; set; }

    public int TotalPresent { get; set; }

    public int TotalAbsent { get; set; }

    public string TotalHours { get; set; }

    public string AvgWorkHours { get; set; }

    public string AvgBreak { get; set; }
}