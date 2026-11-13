using System.ComponentModel.DataAnnotations.Schema;

namespace arna.HRMS.Core.DTOs;

public class LeaveSummaryReportDto
{
    public int? EmployeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? EmployeeName { get; set; }

    [NotMapped]
    public string? ProfileImage { get; set; }
    public string? DepartmentName { get; set; }
    public int? LeaveNameId { get; set; }
    public int? MaxPerYear { get; set; }
    public int? UsedLeave { get; set; }
}
