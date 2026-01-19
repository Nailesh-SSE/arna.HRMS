using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.Entities;

public class EmployeeLeaveBalance : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveMasterId { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }
    public int RemainingLeaves { get; set; }
    public int Year { get; set; }

    public Employee Employee { get; set; }
    public LeaveMaster LeaveMaster { get; set; }
}
