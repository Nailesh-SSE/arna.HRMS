using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class LeaveTypeDto : BaseEntity
{
    public LeaveName LeaveNameId { get; set; }
    public string? Description { get; set; }
    public int MaxPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}
