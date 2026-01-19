using arna.HRMS.Core.Common;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class LeaveRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string ApprovalNotes { get; set; }
    public Employee Employee { get; set; }
    public Employee ApprovedByEmployee { get; set; }
}
