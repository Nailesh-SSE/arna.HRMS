using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Attendance;

public class AttendanceRequestViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? EmployeeName { get; set; }
    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "From date is required.")]
    public DateTime? FromDate { get; set; }

    [Required(ErrorMessage = "To date is required.")]
    public DateTime? ToDate { get; set; }

    [Required(ErrorMessage = "Please select a reason.")]
    public AttendanceReasonType? ReasonTypeId { get; set; }

    [Required(ErrorMessage = "Please select a location.")]
    public AttendanceLocation? LocationId { get; set; }

    [Required(ErrorMessage = "Please select your clock-in time.")]
    public DateTime? ClockIn { get; set; }

    [Required(ErrorMessage = "Please select your clock-out time.")]
    public DateTime? ClockOut { get; set; }

    [Required(ErrorMessage = "Break duration is required.")]
    public TimeSpan? BreakDuration { get; set; }

    public TimeSpan TotalHours { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public Status StatusId { get; set; } = Status.Pending;
    public DateTime? ApprovedOn { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public bool IsApproved { get; set; } = false;
}
