using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Leave;

public class LeaveRequestViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Please select a leave type.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a leave type.")]
    public int LeaveTypeId { get; set; }

    public string? LeaveTypeName { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    public DateTime EndDate { get; set; }

    public int LeaveDays { get; set; }
    public int ReasonId { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string Reason { get; set; } = string.Empty;

    public Status StatusId { get; set; } = Status.Pending;

    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
}
