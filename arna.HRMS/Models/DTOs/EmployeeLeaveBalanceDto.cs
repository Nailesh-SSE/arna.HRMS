using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.DTOs;

public class EmployeeLeaveBalanceDto : CommonDto
{
    public int EmployeeId { get; set; }
    public int LeaveMasterId { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }
    public int RemainingLeaves { get; set; }

}
