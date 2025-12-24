namespace arna.HRMS.Models.ViewModels;

public class ReportViewModel
{
    // Filters
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? EmployeeId { get; set; }
    public string? Department { get; set; }


    // Report results
    public int TotalWorkingDays { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLeaveDays { get; set; }
    public double TotalWorkedHours { get; set; }
}
