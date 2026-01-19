using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.DTOs;

public class LeaveRequestDto : CommonDto
{
    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }   

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalDays { get; set; }

    public string Reason { get; set; }

    public LeaveStatusList Status { get; set; } = LeaveStatusList.Pending;

    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}
