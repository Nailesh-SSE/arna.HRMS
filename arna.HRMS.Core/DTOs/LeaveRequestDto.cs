using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class LeaveRequestDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }

    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }   

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int LeaveDays { get; set; }

    public string Reason { get; set; }

    public Status StatusId { get; set; } = Status.Pending;

    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}
