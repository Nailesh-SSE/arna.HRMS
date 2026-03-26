namespace arna.HRMS.Core.DTOs;

public class EmployeeLeaveDetailsReportDto
{
    public int EmployeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? EmployeeName { get; set; }
    public int LeaveTypeId { get; set; }
    public int LeaveNameId { get; set; }
    public int MaxPerYear { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LeaveDays { get; set; }
    public string? Reason { get; set; }
    public int StatusId { get; set; }
    public int ApprovedBy { get; set; }
    public DateTime ApprovedDate { get; set; }
}
