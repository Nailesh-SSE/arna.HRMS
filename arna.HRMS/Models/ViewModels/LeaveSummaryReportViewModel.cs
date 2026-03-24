namespace arna.HRMS.Models.ViewModels;

public class LeaveSummaryReportViewModel
{
    public int EmpolyeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public int LeaveNameId { get; set; }
    public int MaxPerYear { get; set; }
    public int UsedLeave { get; set; }
}
