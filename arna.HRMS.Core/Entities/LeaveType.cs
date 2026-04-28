using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class LeaveType : BaseEntity
{
    public LeaveName LeaveNameId { get; set; }
    public string? Description { get; set; }
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;

    public ICollection<LeaveRequest> LeaveRequests { get; set; }
}
