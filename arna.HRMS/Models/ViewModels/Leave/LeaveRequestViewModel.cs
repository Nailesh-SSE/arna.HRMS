using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.ViewModels.Leave;

public class LeaveRequestViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int TotalDays { get; set; }

    public string Reason { get; set; }

    public Status Status { get; set; } = Status.Pending;

    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}
