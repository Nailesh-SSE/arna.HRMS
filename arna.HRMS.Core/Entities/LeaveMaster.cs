using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.Entities;

public class LeaveMaster : BaseEntity
{
    public string LeaveName { get; set; }
    public string? Description { get; set; }
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;

    public ICollection<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }
    public ICollection<LeaveRequest> LeaveRequests { get; set; }
}
