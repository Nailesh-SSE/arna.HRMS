using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.ViewModels.Leave;

public class EmployeeLeaveBalanceViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public int LeaveMasterId { get; set; }
    public int TotalLeaves { get; set; }
    public int UsedLeaves { get; set; }
    public int RemainingLeaves { get; set; }
}
