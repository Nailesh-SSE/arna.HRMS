using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.DTOs;

public class EmployeeLeaveBalanceDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public int LeaveMasterId { get; set; }
    public string? LeaveMasterName { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }
    public int RemainingLeaves { get; set; }
}
